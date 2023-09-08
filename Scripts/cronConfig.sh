#!/bin/bash

#######################################################################################################
################################ OPEN SOURCE REQUIREMENTS ############################################
## You must define source of configuration JSON files
## as well as directories for staging and ZIP creation
## of user certificates, truststore certificates, and 
## preference files. 
##
## This script assumes the source of configuration files
## is an Azure storage account, and that the TAK server
## itself lives on an Azure virtual machine which has an
## identity that has read/write access to the 
## storage account.
##
## For example, any references to an Azure storage account
## would be: [blob storage endpoint]/[container name]/[folder name]/configuration files

## Log in to Azure storage account using the TAK VM's identity
echo -e "\n\nLogging into AZ Storage..."
azcopy login --identity

## Copy any new configurations requesting a connection package to a folder on the VM
echo -e "\n\nGetting configurations from AZ Storage..."
azcopy cp "[blob storage endpoint]/[container name]/[new configuration folder name]/" "[TAK server staging folder for new configurations]" --recursive

## Delete copied configurations from azure storage account new configuration folder
echo -e "\n\nRemoving configurations from AZ Storage..."
azcopy rm "[blob storage endpoint]/[container name]/[new configuration folder name]/*" --recursive=true

azcopy logout


FILES="[new configurations staging folder]/*"
## ie. 
dn="[new configurations staging folder]/"
if [ x$(find "$dn" -prune -empty) = x"$dn" ]; then
	echo empty
else
for f in $FILES
do
  echo "Processing $f file..."
  # take action on each file. $f stores current file name
  cat "$f"
  certname=`jq -r '.[].Subject' $f`
  group=`jq -r '.[].GroupIds' $f`
  truststore="[certificate authority truststore name].p12"
  trustpw="[password used to create certificate authority sign]"
  serverip="[public ip of TAK server IP (or VM public IP)]:[TAK server port (typically 8089)]"
  servername="[tak server name]"
  configtype=`jq -r '.[].ConfigType' $f`
  #certname="$c1"
  #group="$g1"
  #echo >> $1
  echo -e "\n\nRead $certname!"

	#read user and group
#if [[ -z ${certname} || -z ${group} ]]; then
 #   echo "Please enter a username and group to assign this certificate. The group name must match a valid group that already within the TAK Server. Contact your IT to create new groups."
 #   exit -1
#fi

## Check if a certificate revoke requiest is present first
if [ $configtype == 2 ]; then
cd /[TAK server directory where cert script files are stored]/
## ie /opt/tak/tak/certs/
bash ./revokeCert.sh $certname ca-do-not-share ca

cd /[TAK folder (typically /opt/tak/)]/
## ie /opt/tak/tak/
docker exec -d takserver-"$(cat tak/version.txt)" bash -c "cd /opt/tak/ && ./configureInDocker.sh"

## change directory back to where configuration files were downloaded to from the Azure storage.
## ie /opt/tak/cron/configurations/
cd /[parent directory of folder containing configuration files]/
#edit config file with blob path and file name
echo -e "\n\nUpdating configuration file for $certname"/
jq -r '.[].StatusCid = 2' $f|sponge $f
jq -r '.[].BlobPath = "'""'"' $f|sponge $f

#move config file to processedConfigs
echo -e "\n\nMoving config to processedConfigs for $certname"
mv $f /[folder where processed configuration files can be stored after completion]/
## ie /opt/tak/tak/cron/processdConfigurations/

#echo -e "\n\nCopying connection package to RevokedPackages in AZ Storage for $certname"
azcopy login --identity

#copy updated config file to AZ storage
echo -e "\n\nCopying updated configuration into AZ storage for $certname"
azcopy cp "[filename of processed configuration].json" "[storage account contianer and folder where revoked certificates should be stored]/RVK-${certname}.json"
## ie opt/tak/cron/processedConfigurations/${certname}.json\

## now processed any certificate service requests for creations and renewals
elif [ $configtype == 1 ] || [ $configtype == 5 ]; then
#create cert file
echo -e "\n\nCreating $certname.p12"

cd /[tak server folder]
## ie /opt/tak/tak/
docker exec takserver-"$(cat /[version.txt file path].txt)" bash -c "cd [cert folder path (typically /opt/tak/tak/certs/)] && ./makeCert.sh client $certname"

#assign cert file to group. If group does not exist, assign
echo -e "\n\nAssigning $certname to group $group"
docker exec takserver-"$(cat /[version.txt file path].txt)" bash -c "cd [TAK server util folder path (typically opt/tak/tak/util/)] && java -jar UserManager.jar certmod -g $group [TAK server cert files folder (typically /opt/tak/tak/cert/files/)]/${certname}.pem"

#Build cert preference file
filename="${certname}.pref"
echo -e "\n\nCreating .pref file for ${filename}"
echo '<?xml version='\''1.0'\'' standalone='\''yes'\''?> ' > ${filename}
echo '<preferences> ' >> ${filename}
echo '<preference version="1" name="cot_streams"> ' >> ${filename}
echo '<entry key="count" class="class java.lang.Integer">1</entry> ' >> ${filename}
echo '<entry key="description0" class="class java.lang.String">'"${servername}"'</entry> ' >> ${filename}
echo '<entry key="enabled0" class="class java.lang.Boolean">true</entry> ' >> ${filename}
echo '<entry key="connectString0" class="class java.lang.String">'"${serverip}"':ssl</entry> ' >> ${filename}
echo '</preference> ' >> ${filename}
echo '<preference version="1" name="com.atakmap.app_preferences"> ' >> ${filename}
echo '<entry key="caLocation" class="class java.lang.String">cert/'"${truststore}"'</entry> ' >> ${filename}
echo '<entry key="caPassword" class="class java.lang.String">'"${trustpw}"'</entry> ' >> ${filename}
echo '<entry key="certificateLocation" class="class java.lang.String">cert/'"${certname}"'.p12</entry> ' >> ${filename}
echo '<entry key="clientPassword" class="class java.lang.String">'"${trustpw}"'</entry> ' >> ${filename}
echo '</preference> ' >> ${filename}
echo '</preferences> ' >> ${filename}

#create fileToZip directory
echo -e "\n\nCreating fileToZip directory for $certname"
## create folder for each configuration to store user certificate, server certificate and pref file
 mkdir /opt/tak/cron/filesToZip/${certname}/

#move pref file to filesToZIp
echo -e "\n\nMoving .pref file to filesToZip for $certname"
mv ${filename} /opt/tak/cron/filesToZip/${certname}/

#copy user and truststore certs to filesToZip
echo -e "\n\nMoving user and truststore certs to filesToZip for $certname"
cp /[TAK server cert files folder (typically /opt/tak/tak/cert/files/)]/${certname}.p12 /opt/tak/cron/filesToZip/${certname}/
cp /[TAK server cert files folder (typically /opt/tak/tak/cert/files/)]/${truststore} /opt/tak/cron/filesToZip/${certname}/

#make connectionPackages directory
echo -e "\n\nCreating fileToZip directory for $certname"
mkdir /opt/tak/cron/connectionPackages/${certname}/

#zip up filesToZip and put in connectionPackages directory
echo -e "\n\nZipping filesToZip into connectionPackages for $certname"
cd /opt/tak/cron/filesToZip/${certname}/ && zip -r /opt/tak/cron/connectionPackages/${certname}/${certname}.zip  ./* && cd -

cd /opt/tak/tak/

cpblobpath="ConnectionPackages/${certname}/${certname}.zip"
updatedconfigblobpath="ProcessedConfigs/${certname}.json"

#edit config file with blob path and file name
echo -e "\n\nUpdating configuration file for $certname"
jq -r '.[].FileName = "'"${certname}"'"' $f|sponge $f
jq -r '.[].StatusCid = 2' $f|sponge $f
jq -r '.[].BlobPath = "'"${updatedconfigblobpath}"'"' $f|sponge $f

#move config file to processedConfigs
echo -e "\n\nMoving config to processedConfigs for $certname"
mv $f /opt/tak/cron/processedConfigs/

#copy zip file to AZ storage
echo -e "\n\nCopy connection package to ConnectionPackages in AZ Storage for $certname"
azcopy login --identity
azcopy cp "opt/tak/cron/connectionPackages/${certname}/${certname}.zip" "[blob storage endpoint]/[container name]/[new connection package folder name]/${certname}/${certname}.zip"

#copy updated config file to AZ storage
echo -e "\n\nCopying updated configuration into AZ storage for $certname"
azcopy cp "opt/tak/cron/processedConfigs/${certname}.json" "[blob storage endpoint]/[container name]/[processed certificate requests folder name]/${certname}.json"

fi
azcopy logout

done

fi


# Database Configuration

## MS SQL Server

TAKAM is designed to work with a Microsoft SQL server for storing user, certificate request, and group data pertaining to a TAK server.

The following SQL CREATE scripts will create the required database(s) and tables required for out-of-the-box development and functionality after cloning the TAKAM repository.

## SQL CREATE Commands

1. Database **TAKAccessManager**
2. Code Table **CfgTypeCodes** (Configuration Type Codes)
3. Code Table **PkgStatusCodes** (Connection Package Status Codes)
4. User Table **AgencyAdministrators**
5. User Table **TakUsers**
6. User Table **TakAgencies**
7. File Table **Configurations** (Certificate Service Request JSON file information)
8. File Table **DataPackages** (Connection Package ZIP file information)

## Creating the Database

Within your SQL Server management tool (SSMS or Azure SQL is recommended) run the following SQL CREATE script to create the database.

```SQL
CREATE DATABASE [TAKAccessManager]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'TAKAccessManager', FILENAME = N'D:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\TAKAccessManager.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 10%)
 LOG ON 
( NAME = N'TAKAccessManager_log', FILENAME = N'E:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\Data\TAKAccessManager_log.ldf' , SIZE = 1024KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [TAKAccessManager].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO

ALTER DATABASE [TAKAccessManager] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [TAKAccessManager] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [TAKAccessManager] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [TAKAccessManager] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [TAKAccessManager] SET ARITHABORT OFF 
GO
ALTER DATABASE [TAKAccessManager] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [TAKAccessManager] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [TAKAccessManager] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [TAKAccessManager] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [TAKAccessManager] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [TAKAccessManager] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [TAKAccessManager] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [TAKAccessManager] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [TAKAccessManager] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [TAKAccessManager] SET  DISABLE_BROKER 
GO
ALTER DATABASE [TAKAccessManager] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [TAKAccessManager] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [TAKAccessManager] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [TAKAccessManager] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [TAKAccessManager] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [TAKAccessManager] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [TAKAccessManager] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [TAKAccessManager] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [TAKAccessManager] SET  MULTI_USER 
GO
ALTER DATABASE [TAKAccessManager] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [TAKAccessManager] SET DB_CHAINING OFF 
GO
ALTER DATABASE [TAKAccessManager] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [TAKAccessManager] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
ALTER DATABASE [TAKAccessManager] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [TAKAccessManager] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [TAKAccessManager] SET QUERY_STORE = OFF
GO
ALTER DATABASE [TAKAccessManager] SET  READ_WRITE 
GO
```
## Code Tables

```SQL
CREATE TABLE [dbo].[CfgTypeCodes](
	[TypeCode] [int] IDENTITY(1,1) NOT NULL,
	[TypeDescription] [varchar](255) NULL,
	[Active] [int] NULL
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[PkgStatusCodes](
	[StatusCode] [int] IDENTITY(1,1) NOT NULL,
	[StatusDescription] [varchar](255) NULL,
	[Active] [int] NULL
) ON [PRIMARY]
GO

SET IDENTITY_INSERT [dbo].[CfgTypeCodes] ON 
GO
INSERT [dbo].[CfgTypeCodes] ([TypeCode], [TypeDescription], [Active]) VALUES (1, N'UserCreate', 1)
GO
INSERT [dbo].[CfgTypeCodes] ([TypeCode], [TypeDescription], [Active]) VALUES (2, N'Revoke', 1)
GO
INSERT [dbo].[CfgTypeCodes] ([TypeCode], [TypeDescription], [Active]) VALUES (3, N'GroupCreate', 0)
GO
INSERT [dbo].[CfgTypeCodes] ([TypeCode], [TypeDescription], [Active]) VALUES (4, N'EventCreate', 0)
GO
INSERT [dbo].[CfgTypeCodes] ([TypeCode], [TypeDescription], [Active]) VALUES (5, N'Renew', 1)
GO
SET IDENTITY_INSERT [dbo].[CfgTypeCodes] OFF
GO
SET IDENTITY_INSERT [dbo].[PkgStatusCodes] ON 
GO
INSERT [dbo].[PkgStatusCodes] ([StatusCode], [StatusDescription], [Active]) VALUES (1, N'Requested', 1)
GO
INSERT [dbo].[PkgStatusCodes] ([StatusCode], [StatusDescription], [Active]) VALUES (2, N'Available', 1)
GO
INSERT [dbo].[PkgStatusCodes] ([StatusCode], [StatusDescription], [Active]) VALUES (3, N'Revoked', 1)
GO
INSERT [dbo].[PkgStatusCodes] ([StatusCode], [StatusDescription], [Active]) VALUES (4, N'Expired', 1)
GO
SET IDENTITY_INSERT [dbo].[PkgStatusCodes] OFF
GO
```

### Configuration Type Codes
The CfgTypeCodes table stores info about the configuration types that can be used in creating a certificate service request. With each generation of a certificate service request (CSR) a JSON file is created by the application to be sent to the TAK server. The TAK server reads every JSON file created per CSR and processes the command indicated by the configuration type. A JSON file can indicated any of the following actions: 
1. **UserCreate** - Create a connection package for a new user
2. **Revoke** - Revoke an existing certificate's validity against a TAK server
3. **GroupCreate** - Create a new group on the TAK server which certificates can be assigned to
4. **EventCreate** - Not yet in use
5. **Renew** - Create a new certificate to replace a certificate which has expired

### Package Status Codes
The PkgStatusCodes table stores info about the connection package statuses that apply to an connection package at any given time. NOTE: With regards to TAKAM, a "connection package" is a ZIP file that contains a user certificate, server certificate, and preference file- all of which are required for connection to a TAK server configured to use flat-file authentication.
1. **Requested** - A certificate has been requested by a user and is in queue to be processed by a TAK server
2. **Available** - A certificate has been created by a TAK server and is available to be downloaded through the TAKAM application
3. **Revoked** - A certificate has been manually revoked through the TAKAM application. Certificates that have been revoked through the TAK server will not be captured in the database.
4. **Expired** - A certificate that has been created in TAKAM has expired, and is no longer usable to connect to a TAK server. This file should be made renewable unless indicated otherwise in the TAKAM application.

## User Tables

```SQL
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TakUsers](
	[UserId] [uniqueidentifier] NOT NULL,
	[UserName] [varchar](100) NULL,
	[LastLogon] [datetime] NULL,
	[PrimaryGroup] [int] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[LastModified] [datetime] NOT NULL,
	[Active] [bit] NOT NULL,
 CONSTRAINT [PK_TakUsers] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
```

### TAK Users
The TakUsers table stores information about individual users as they first sign into TAKAM and request/create/renew/revoke their connection packages. A new dataset is created for a user when they first sign in, and a user's "Last Logon" date is updated with each sign in.

```SQL
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[AgencyAdministrators](
	[AgencyId] [int] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[AssignedDate] [datetime] NOT NULL,
	[DeactivationDate] [datetime] NULL,
	[Active] [int] NOT NULL,
	[GroupOnly] [bit] NULL
) ON [PRIMARY]
GO
```

### Agency Administrators
TAKAM is designed to support organization heirarchies at multiple levels (from top to bottom): Consortium [Admin], Agency, Teams/Units, Users. An agency administrator is a user-level individual who has the authority to manage access to a TAK server accross an organization. This includes revoking exisiting certificates and revoking a user's access to the sytem overall. Currently, this table must be configured manually. Administrators must have their record on this table created by a consortium administrator via their SQL server management tool of choice, and this requires obtaining a user's userid guid from the TakUsers table after that user has signed in at least once.

```SQL
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TakGroups](
	[GroupId] [int] IDENTITY(1,1) NOT NULL,
	[GroupName] [varchar](50) NOT NULL,
	[GroupContactName] [varchar](255) NULL,
	[GroupContactNumber] [varchar](50) NULL,
	[GroupContactEmail] [varchar](255) NULL,
	[Active] [bit] NOT NULL,
	[CreateDt] [datetime] NOT NULL,
	[CreateUserId] [uniqueidentifier] NOT NULL,
	[LastModifiedDt] [datetime] NULL,
	[AgencyId] [int] NULL,
	[InviteOnly] [int] NULL,
 CONSTRAINT [PK_TakGroups] PRIMARY KEY CLUSTERED 
(
	[GroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
```

### TAK Groups
TAK groups directly translate to groups existing in a TAK server. A group created in TAKAM is assigned an identifier (integer) which is used as the group name on a TAK server. This is done for security purposes, and can be changed to assign a group on a TAK server the same name it is given in TAKAM.

## File Tables 

```SQL
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Configurations](
	[ConfigId] [int] IDENTITY(1,1) NOT NULL,
	[Subject] [varchar](50) NULL,
	[GroupIds] [varchar](255) NULL,
	[ConfiguredBy] [varchar](50) NULL,
	[ConfigureDate] [datetime] NULL,
	[ExpirationDate] [datetime] NULL,
	[Notes] [varchar](max) NULL,
	[Server] [varchar](50) NULL,
	[StatusCid] [int] NULL,
	[PackageId] [int] NULL,
	[UserId] [uniqueidentifier] NULL,
	[FileName] [varchar](100) NULL,
	[BlobPath] [varchar](max) NULL,
	[ConfigType] [int] NULL,
 CONSTRAINT [PK_Configurations] PRIMARY KEY CLUSTERED 
(
	[ConfigId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
```

### Configurations
With regards to TAKAM, a configuration is synonymous with a certificate service request. TAKAM enables an end user to request a connection package zip file that can be used to connect to a TAK server from iTAK/WinTAK/ATAK. This table records all transactions pertaining to a connection package, including certificate requests, creation, revoking, renewing, and modifying.

```SQL
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[DataPackages](
	[PackageId] [int] IDENTITY(1,1) NOT NULL,
	[PackageName] [varchar](50) NULL,
	[GroupIds] [varchar](255) NULL,
	[Server] [varchar](50) NULL,
	[Notes] [varchar](max) NULL,
	[BlobPath] [varchar](max) NULL,
	[StatusCid] [int] NULL,
	[UserId] [uniqueidentifier] NULL,
	[ExpirationDate] [datetime] NULL,
	[ConfigureDt] [datetime] NULL,
	[ParentPkgId] [int] NULL,
	[Renewed] [bit] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[DataPackages] ADD  CONSTRAINT [DF_DataPackages_Renewed]  DEFAULT ((0)) FOR [Renewed]
GO
```

### Data Packages
Data packages are the ZIP files resulting from CSRs processed by a TAK server, and can be used to connect to a TAK server in iTAK/WinTAK/ATAK.

## Post Script Setup
After each of the required tables have been created and populated, a connection string reference must be created in the appsettings.json of the TAKAM application.
Note that TAKAM could technically be refactored to work with a No-SQL database, as ZIP and JSON files are the primary data object types used for regular operations.

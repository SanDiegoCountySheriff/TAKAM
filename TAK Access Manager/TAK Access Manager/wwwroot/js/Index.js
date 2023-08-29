$(document)
    .ajaxStart(function () {
        $('#createNewGroupLoading').show();
        var elementVar = document.getElementById("btnAddGroup");
        elementVar.setAttribute("loading", "true")
        var elementVar2 = document.getElementById("slBtnAddUser");
        elementVar2.setAttribute("loading", "true");
    })
    .ajaxStop(function () {
        $('#createNewGroupLoading').hide();
        var elementVar = document.getElementById("btnAddGroup");
        elementVar.removeAttribute("loading");
        var elementVar2 = document.getElementById("slBtnAddUser");
        elementVar2.removeAttribute("loading");
    });

$(document).ready(function () {
    $('#loading').hide();
    $('[data-toggle="popover"]').popover();

});

function postConfig() {
    let configData = {
        subject: $('#slInputCertFileName').val(),
        GroupIds: $('#slConfigSelectGroup').val(),
        configuredBy: $('#slInputCurrUser').attr('placeholder'),
        notes: $('#slInputUserNotes').val(),
        server: $('#slSelectServer').val(),
    };

    $.ajax({
        type: 'POST',
        url: `/api/User/PostConfig/`,
        data: configData,
        xhrFields: {
            withCredentials: true
        },
        success: function (response) {
            toastr.options.showDuration = 300;
            toastr.options.hideDuration = 1000;
            toastr.options.timeOut = 20000;
            toastr.options.extendedTimeOut = 10000;
            toastr.options.showMethod = 'slideDown';
            toastr.options.hideMethod = 'slideUp';
            toastr.options.closeMethod = 'slideUp';
            toastr.options.newestOnTop = false;
            toastr.options.positionClass = 'toast-top-left';
            toastr.info("User file successfully created");

        },
        error: function (response) {
            toastr.options.showDuration = 300;
            toastr.options.hideDuration = 1000;
            toastr.options.timeOut = 20000;
            toastr.options.extendedTimeOut = 10000;
            toastr.options.showMethod = 'slideDown';
            toastr.options.hideMethod = 'slideUp';
            toastr.options.closeMethod = 'slideUp';
            toastr.options.newestOnTop = false;
            toastr.options.positionClass = 'toast-top-left';
            toastr.error("An error occurred creating your file.");
        }
    }).then(function () {
        $('#newConfigModal').removeClass('fade');
        $('#newConfigModal').modal('hide');
        $('#slInputCertName').val('');
        $('#slConfigSelectUser').val('');
        $('#slConfigSelectGroup').val('');
        $('#slInputCertFileName').val('');
        $('#slInputUserNotes').val('');
    });
}

function postNewGroup() {

    let groupData = {
        groupName: $('#inputGroupName').val(),
        groupContactEmail: $('#inputGroupAdmin').val()
    };

    $.ajax({
        type: 'POST',
        url: `/api/Admin/PostGroup/`,
        data: groupData,
        xhrFields: {
            withCredentials: true
        },
        success: function (response) {
            toastr.options.showDuration = 300;
            toastr.options.hideDuration = 1000;
            toastr.options.timeOut = 20000;
            toastr.options.extendedTimeOut = 10000;
            toastr.options.showMethod = 'slideDown';
            toastr.options.hideMethod = 'slideUp';
            toastr.options.closeMethod = 'slideUp';
            toastr.options.newestOnTop = false;
            toastr.options.positionClass = 'toast-top-left';
            toastr.info("Group successfully created");

        },
        error: function (response) {
            toastr.options.showDuration = 300;
            toastr.options.hideDuration = 1000;
            toastr.options.timeOut = 20000;
            toastr.options.extendedTimeOut = 10000;
            toastr.options.showMethod = 'slideDown';
            toastr.options.hideMethod = 'slideUp';
            toastr.options.closeMethod = 'slideUp';
            toastr.options.newestOnTop = false;
            toastr.options.positionClass = 'toast-top-left';
            toastr.error("An error occurred creating your group.");
        },
    }).then(() => {
        $('#onboardUnitModal').removeClass('fade');
        $('#onboardUnitModal').modal('hide');
        $('#slgroupname').val('');
        $('#inputGroupName').val('');
        $('#inputGroupAdmin').val('');
    });
}

$(document).ready(function () {
    var text = '';
    var gnameSpan2 = '';
    var text3 = '';
    var gnameSpan3 = '';
    let singleSelect = document.querySelector("#slSelectAgency");
    let configGroupSelect = document.querySelector("#slConfigSelectGroup");

    const containerExtAgencyAlert = document.querySelector('.animation-fadein-alertExtAgency');
    const animationExtAgencyAlert = containerExtAgencyAlert.querySelector('sl-animation');


    document.getElementById("slgroupname").onkeyup = function () {
        text = $("#slgroupname").val();
        gnameSpan2 = singleSelect.value + "-" + text;
        document.getElementById('inputGroupName').value = gnameSpan2;
    };

    singleSelect.addEventListener("sl-change", () => {
        text = $("#slgroupname").val();
        gnameSpan2 = singleSelect.value + "-" + text;
        document.getElementById('inputGroupName').value = gnameSpan2;

        if (singleSelect.value == 'EXT') {
            $('#alertExtAgency').attr('hidden', false);
            animationExtAgencyAlert.play = true;
        }
        else {
            $('#alertExtAgency').attr('hidden', true);
            animationExtAgencyAlert.play = false;
        }
    });

    const containeralertNonAdmin = document.querySelector('.animation-fadein-alertNonAdmin');
    const animationalertNonAdmin = containeralertNonAdmin.querySelector('sl-animation');

    configGroupSelect.addEventListener("sl-change", () => {
        text3 = $("#slInputCertName").val();
        gnameSpan3 = configGroupSelect.value + "-" + text3;

        document.getElementById('slInputCertFileName').value = gnameSpan3;

        if (singleSelect.value === 'EXT') {
            $('#alertNotAdmin').attr('hidden', false);
            animationalertNonAdmin.play = true;
        }
        else {
            $('#alertNotAdmin').attr('hidden', true);
            animationalertNonAdmin.play = false;
        }
    }); 

});

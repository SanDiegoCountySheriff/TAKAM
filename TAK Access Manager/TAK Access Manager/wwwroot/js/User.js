function postConfig() {
    let configData = {
        subject: $('#inputPackageName').attr('placeholder'),
        groupIds: $('#slConfigSelectGroup').attr('placeholder'),
        configuredBy: $('#slInputCurrUser').attr('placeholder'),
        notes: $('#slInputUserNotes').val(),
        server: $('#slSelectServer').attr('placeholder'),
        configType: 1,
        expirationDate: $('#createExpirationDt').attr('placeholder')
    }; 

    $.ajax({
        type: 'POST',
        url: `/User/PostConfig/`,
        data: configData,
        xhrFields: {
            withCredentials: true
        },
        success: function (response) {
            location.reload();

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
    });
}

function revokeConfig() {
    let configData = {
        subject: $('#slselectRevokeCert').val(),
        configuredBy: $('#slRevokeInputCurrUser').attr('placeholder'),
        configType: 2
    };

    $.ajax({
        type: 'POST',
        url: `/Admin/PostConfig/`,
        contentType: 'application/json',
        data: configData,
        xhrFields: {
            withCredentials: true
        },
        success: function (response) {
            location.reload();

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
            toastr.error("An error occurred revoking your file.");
        }
    });
}

function reissueConfig() {
    var subjectId = $('#slselectRenewCert').val();
    var subjectName = $('#slselectRenewCert').text();
    subjectName = subjectName.replace(/\n/g, '');
    subjectName = subjectName.replace(/ /g, '');
    let reissueData = {
        subject: subjectId,
        groupIds: null,
        configuredBy: null,
        notes: null,
        server: "CSSA Production",
        configType: 5,
        fileName: subjectName,
        expirationDate: $('#renewExpirationDt').attr('placeholder'),
    };

    $.ajax({
        type: 'POST',
        url: `/User/PostConfig/`,
        data: reissueData,
        xhrFields: {
            withCredentials: true
        },
        success: function (response) {
            location.reload();

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
            toastr.error("An error occurred revoking your file.");
        }
    });

}

$(document).ready(function () {
});
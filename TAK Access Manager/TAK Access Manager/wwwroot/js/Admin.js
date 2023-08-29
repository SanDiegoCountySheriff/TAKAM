function revokeConfig() {
    let configData = {
        subject: $('#slselectRevokeCert').val(),
        configuredBy: $('#slRevokeInputCurrUser').attr('placeholder'),
        configType: 2
    };
    let accountToDeactivate = $('#slselectDisableAccnt').val()
    

    $.ajax({
        type: 'POST',
        url: `/api/Admin/PostConfig/`,
        data: configData,
        xhrFields: {
            withCredentials: true
        },
        success: function () {
            $.ajax({
                type: 'POST',
                url: `/api/Admin/DeactivateAccount/${accountToDeactivate}`,
                data: accountToDeactivate,
                xhrFields: {
                    withCredentials: true
                },
                success: function () {
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
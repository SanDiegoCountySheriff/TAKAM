$(document).ready(function () {
    $('[data-toggle="popover"]').popover();
});

function postUserSettings() {

    let userData = {
        userId: $('#currUserId').attr('placeholder'),
        userName: $('#currUser').attr('placeholder'),
        groupId: $('#groupInputAdmin').val(),

    };

    $.ajax({
        type: 'POST',
        url: `/api/Admin/PostUserSettings/`,
        data: userData,
        xhrFields: {
            withCredentials: true
        }
    }).then(function (response) {
        $('#userSettingsModal').modal('toggle');
    });
}
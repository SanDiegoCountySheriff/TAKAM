
$(document).ready(function () {
    let currGroupSelect = document.querySelector("#slselAnotherGroup");


    currGroupSelect.addEventListener("sl-change", () => {
        var newGroupId = currGroupSelect.value;
        var baseUrl = `/Group/GroupView/${newGroupId}`;
        window.location.href = baseUrl;
    });

});


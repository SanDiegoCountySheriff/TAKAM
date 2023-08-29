$(document).ready(function () {
    let createGroupSwitch = document.querySelector("#createGroupSwitch");
    var text = '';
    var gnameSpan2 = '';
    let singleSelect = document.querySelector("#slSelectAgency");

    createGroupSwitch.addEventListener("sl-change", () => {
        var elementVar = document.getElementById("createGroupTreeItem");
        elementVar.toggleAttribute("hidden");

        var disabledGroupSelect = document.getElementById("configSelectGroup");
        disabledGroupSelect.toggleAttribute("disabled");
        disabledGroupSelect.toggleAttribute("expanded");
    });

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

});
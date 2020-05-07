// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function uploadFiles(inputId) {
    var input = document.getElementById(inputId);

    var formData = new FormData();
    formData.append('file', $('input[type=file]')[0].files[0]);

    startUpdatingProgressIndicator();

    $.ajax(
        {
            url: "/Upload",
            data: formData,
            processData: false,
            contentType: false,
            type: "POST",
            success: function (data) {
                stopUpdatingProgressIndicator();
                $('#successtoast').toast({ delay: 10000 });
                $('#successtoast').toast('show');
                $('#defaultprofilephoto').hide();
                $('#uploadedprofilephoto').show();
                $('#uploadedprofilephoto').attr("src", "/Upload/?filename=" + data);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                $('#failuretoast').toast({ delay: 10000 });
                $('#failuretoast').toast('show');
            }
        }
    );
}

$('body').on('click', '.close', function () {
    $(this).closest('.toast').toast('hide');
});

var intervalId;

function startUpdatingProgressIndicator() {
    $("#progress").show();

    intervalId = setInterval(
        function () {
            $.post(
                "/Upload/Progress",
                function (progress) {
                    $("#bar").css({ width: progress + "%" });
                    $("#label").html(progress + "%");
                }
            );
        },
        10
    );
}

function stopUpdatingProgressIndicator() {
    clearInterval(intervalId);
}

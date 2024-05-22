// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

//TODO: use SignalR?
//$(document).ready(() => {
//    var pollingEnabled = true;  

//    function fetchState() {
//        // Check if polling is enabled
//        if (!pollingEnabled) return;

//        $.ajax({
//            type: "GET",
//            url: "/VirtualMachine/GetState",
//            //data: "{}",
//            contentType: "application/json; charset=utf-8",
//            dataType: "json",
//            async: true,
//            success: function (result) {
//                $("#status").empty();
//                $.each(result, (key, item) => {
//                    $("#status").append('<p>' + item + '</p>')
//                });
//                setTimeout(fetchState, 1000);
//            },
//            error: function (errormessage) {
//                $("#h3").text(errormessage.responseText);
//                return false;
//            }
//        });
//    }

//    // Function to check and update polling status based on backend condition
//    function checkPollingStatus() {
//        $.ajax({
//            type: "GET",
//            url: "/VirtualMachine/GetPollingStatus",
//            //data: "{}",
//            contentType: "application/json; charset=utf-8",
//            dataType: "json",
//            async: true,
//            success: function (response) {
//                // If polling is enabled, start polling
//                if (pollingEnabled) {
//                    fetchState();
//                }

//                pollingEnabled = response.pollingEnabled; // Update polling status based on backend response
//                setTimeout(checkPollingStatus, 5000);
//            },
//            error: function (xhr, status, error) {
//                // Handle error
//                console.error("Error fetching polling status:", error);
//            }
//        });
//    }

//    checkPollingStatus();
//});

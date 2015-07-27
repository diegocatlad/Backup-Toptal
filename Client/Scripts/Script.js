$(document).ready(function () {
    var mainContent = $("#mainContent");

    function ShowMessage(isError, message) {
        if (isError) {
            $("#lblError").html(message);
            $("#lblError").show().delay(5000).fadeOut();
            $("#lblSuccess").hide();
        }
        else {
            $("#lblSuccess").html(message);
            $("#lblSuccess").show().delay(5000).fadeOut();
            $("#lblError").hide();
        }
    }

    function ShowHideActionLinks(role) {
        switch (role) {
            case "Administrator":
                $('#lnkRegister').hide();
                $('#lnkLogin').hide();
                $('#lnkLogoff').show();
                $('#lnkUsersList').show();
                $('#lnkTripsList').show();
                break;
            case "Manager":
                $('#lnkRegister').hide();
                $('#lnkLogin').hide();
                $('#lnkLogoff').show();
                $('#lnkUsersList').show();
                $('#lnkTripsList').hide();
                break;
            case "User":
                $('#lnkRegister').hide();
                $('#lnkLogin').hide();
                $('#lnkLogoff').show();
                $('#lnkUsersList').hide();
                $('#lnkTripsList').show();
                break;
                // Not logged in
            default:
                $('#lnkRegister').show();
                $('#lnkLogin').show();
                $('#lnkLogoff').hide();
                $('#lnkUsersList').hide();
                $('#lnkTripsList').hide();
        }
    }

    function LoadTripsList() {
        $.ajax({
            type: "GET",
            url: URL_TripsList,
            data: "{}",
            cache: false,
            dataType: "html",
            success: function (data) {
                mainContent.html(data);
            }
        })
    };

    function LoadUsersList() {
        $.ajax({
            type: "GET",
            url: URL_UsersList,
            data: "{}",
            cache: false,
            dataType: "html",
            success: function (data) {
                mainContent.html(data);
            }
        })
    };


    $("#btncancel").live("click", function (e) {
        mainContent.html("");
    });


    $("#lnkRegister").live("click", function (e) {
        $.ajax({
            type: "GET",
            url: URL_Register,
            data: "{}",
            cache: false,
            dataType: "html",
            success: function (data) {
                mainContent.html(data);
            }
        })
        return false;
    });

    $("#btnRegister").live("click", function (e) {
        $(this).dialog("close");
        var token = $('input[name="__RequestVerificationToken"]').val();
        var username = $("#frmRegister #Username").val();
        var password = $("#frmRegister #Password").val();
        var confirmPassword = $("#frmRegister #ConfirmPassword").val();
        $.ajax({
            type: "POST",
            url: URL_Register,
            data: {
                __RequestVerificationToken: token,
                Username: username,
                Password: password,
                ConfirmPassword: confirmPassword
            },
            dataType: "json",
            success: function (data) {
                if (data.Error) {
                    ShowMessage(true, data.Message)
                }
                else {
                    ShowHideActionLinks(data.Role);
                    mainContent.html("");
                }
            },
            error: function (data) {
                ShowMessage(true, "Unexpected error occurred. Please try again later or contact your system administrator");
            }
        })
    });

    $("#lnkLogin").live("click", function (e) {
        $.ajax({
            type: "GET",
            url: URL_Login,
            data: "{}",
            cache: false,
            dataType: "html",
            success: function (data) {
                mainContent.html(data);
            }
        })
        return false;
    });

    $("#btnLogin").live("click", function (e) {
        $(this).dialog("close");
        var token = $('input[name="__RequestVerificationToken"]').val();
        var username = $("#frmLogin #Username").val();
        var password = $("#frmLogin #Password").val();
        $.ajax({
            type: "POST",
            url: URL_Login,
            data: {
                __RequestVerificationToken: token,
                Username: username,
                Password: password
            },
            dataType: "json",
            success: function (data) {
                if (data.Error) {
                    ShowMessage(true, data.Message)
                }
                else {
                    ShowHideActionLinks(data.Role);
                    mainContent.html("");
                }
            },
            error: function (data) {
                ShowMessage(true, "Unexpected error occurred. Please try again later or contact your system administrator");
            }
        })
    });

    $("#lnkLogoff").live("click", function (e) {
        $.ajax({
            type: "POST",
            url: URL_Logoff,
            data: {
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            cache: false,
            dataType: "html",
            success: function (data) {
                if (data.Error) {
                    ShowMessage(true, data.Message)
                }
                else {
                    ShowHideActionLinks("");
                    mainContent.html("");
                }
            }
        })
        return false;
    });

    
    $("#lnkUsersList").live("click", function (e) {
        LoadUsersList();
        return false;
    });

    $("#lnkCreateUser").live("click", function (e) {
        $.ajax({
            type: "GET",
            url: URL_CreateUser,
            data: "{}",
            cache: false,
            dataType: "html",
            success: function (data) {
                mainContent.html(data);
            },
            error: function (data) {
                ShowMessage(true, "Unexpected error occurred. Please try again later or contact your system administrator");
            }
        })
        return false;
    });

    $("#btnSaveCreateUser").live("click", function (e) {
        var token = $('input[name="__RequestVerificationToken"]').val();
        var username = $("#frmCreateUsers #User_Username").val();
        var password = $("#frmCreateUsers #User_Password").val();
        var confirmPassword = $("#frmCreateUsers #User_ConfirmPassword").val();
        var roleId = $("#frmCreateUsers #User_Role_Id").val();

        $.ajax({
            type: "POST",
            url: URL_CreateUser,
            data: {
                __RequestVerificationToken: token,
                Username: username,
                Password: password,
                ConfirmPassword: confirmPassword,
                RoleId: roleId
            },
            success: function (data) {
                if (data.Error) {
                    ShowMessage(true, data.Message);
                }
                else {
                    LoadUsersList();
                }
            },
            error: function (data) {
                ShowMessage(true, "Unexpected error occurred. Please try again later or contact your system administrator");
            }
        })
    });

    $("#btnCancelUserEdition").live("click", function (e) {
        LoadUsersList();
    });

    $(".lnkEditUser").live("click", function (e) {
        $.ajax({
            type: "GET",
            url: URL_EditUser,
            data: { id: $(e.srcElement).attr("item-id") },
            cache: false,
            dataType: "html",
            success: function (data) {
                mainContent.html(data);
            },
            error: function (data) {
                ShowMessage(true, "Unexpected error occurred. Please try again later or contact your system administrator");
            }
        })
        return false;
    });

    $("#btnSaveEditUser").live("click", function (e) {
        var token = $('input[name="__RequestVerificationToken"]').val();

        var id = $("#frmEditUsers #User_Id").val();
        var username = $("#frmEditUsers #User_Username").val();
        var password = $("#frmEditUsers #User_Password").val();
        var confirmPassword = $("#frmEditUsers #User_ConfirmPassword").val();
        var roleId = $("#frmEditUsers #User_RoleId").val();

        $.ajax({
            type: "POST",
            url: URL_EditUser,
            data: {
                __RequestVerificationToken: token,
                Id: id,
                Username: username,
                Password: password,
                ConfirmPassword: confirmPassword,
                RoleId: roleId
            },
            dataType: "json",
            success: function (data) {
                if (data.Error) {
                    ShowMessage(true, data.Message)
                }
                else {
                    LoadUsersList();
                }
            },
            error: function (data) {
                ShowMessage(true, "Unexpected error occurred. Please try again later or contact your system administrator");
            }
        })
    });

    $(".lnkDeleteUser").live("click", function (e) {
        var id = $(e.srcElement).attr("item-id");
        $("#dialog-confirm").dialog({
            resizable: false,
            modal: true,
            buttons: {
                Yes: function () {
                    var token = $('input[name="__RequestVerificationToken"]').val();
                    $.ajax({
                        type: "POST",
                        url: URL_DeleteUser,
                        data: {
                            __RequestVerificationToken: token,
                            id: id
                        },
                        cache: false,
                        dataType: "json",
                        success: function (data) {
                            if (data.Error) {
                                ShowMessage(true, data.Message)
                            }
                            else {
                                $("#btnFilterUser").click();
                            }
                        },
                        error: function (data) {
                            ShowMessage(true, "Unexpected error occurred. Please try again later or contact your system administrator");
                        }
                    });
                    $(this).dialog("close");
                },
                No: function () {
                    $(this).dialog("close");
                }
            }
        });
        return false;
    });

    $("#btnFilterUser").live("click", function (e) {
        $.ajax({
            type: "GET",
            url: URL_FilterUser,
            data: { Username: $("#Username").val() },
            cache: false,
            dataType: "html",
            success: function (data) {
                $("#divUsersList").html(data);
            }
        });
    });

    $("#btnClearFilterUser").live("click", function (e) {
        $.ajax({
            type: "GET",
            url: URL_FilterUser,
            data: { Username: "" },
            cache: false,
            dataType: "html",
            success: function (data) {
                $("#divUsersList").html(data);
            }
        });
    });



    $("#lnkTripsList").live("click", function (e) {
        LoadTripsList();
        return false;
    });

    $("#lnkCreate").live("click", function (e) {
        $.ajax({
            type: "GET",
            url: URL_Create,
            data: "{}",
            cache: false,
            dataType: "html",
            success: function (data) {
                mainContent.html(data);
            },
            error: function (data) {
                ShowMessage(true, "Unexpected error occurred. Please try again later or contact your system administrator");
            }
        })
        return false;
    });

    $("#btnSaveCreate").live("click", function (e) {
        var token = $('input[name="__RequestVerificationToken"]').val();
        var destination = $("#frmCreateTrips #Destination").val();
        var startDate = $("#frmCreateTrips #StartDate").val();
        var endDate = $("#frmCreateTrips #EndDate").val();
        var comment = $("#frmCreateTrips #Comment").val();

        $.ajax({
            type: "POST",
            url: URL_Create,
            data: {
                __RequestVerificationToken: token,
                Destination: destination,
                StartDate: new Date(startDate).toUTCString(),
                EndDate: new Date(endDate).toUTCString(),
                Comment: comment
            },
            success: function (data) {
                if (data.Error) {
                    ShowMessage(true, data.Message);
                }
                else {
                    LoadTripsList();
                }
            },
            error: function (data) {
                ShowMessage(true, "Unexpected error occurred. Please try again later or contact your system administrator");
            }
        })
    });

    $("#btnCancelTripEdition").live("click", function (e) {
        LoadTripsList();
    });

    $(".lnkEdit").live("click", function (e) {
        $.ajax({
            type: "GET",
            url: URL_Edit,
            data: { id: $(e.srcElement).attr("item-id") },
            cache: false,
            dataType: "html",
            success: function (data) {
                mainContent.html(data);
            },
            error: function (data) {
                ShowMessage(true, "Unexpected error occurred. Please try again later or contact your system administrator");
            }
        })
        return false;
    });

    $("#btnSaveEdit").live("click", function (e) {
        var token = $('input[name="__RequestVerificationToken"]').val();

        var id = $("#frmEditTrips #Id").val();
        var userId = $("#frmEditTrips #UserId").val();
        var destination = $("#frmEditTrips #Destination").val();
        var startDate = $("#frmEditTrips #StartDate").val();
        var endDate = $("#frmEditTrips #EndDate").val();
        var comment = $("#frmEditTrips #Comment").val();

        $.ajax({
            type: "POST",
            url: URL_Edit,
            data: {
                __RequestVerificationToken: token,
                Id: id,
                UserId: userId,
                Destination: destination,
                StartDate: new Date(startDate).toUTCString(),
                EndDate: new Date(endDate).toUTCString(),
                Comment: comment
            },
            dataType: "json",
            success: function (data) {
                if (data.Error) {
                    ShowMessage(true, data.Message)
                }
                else {
                    LoadTripsList();
                }
            },
            error: function (data) {
                ShowMessage(true, "Unexpected error occurred. Please try again later or contact your system administrator");
            }
        })
    });

    $(".lnkDelete").live("click", function (e) {
        var id = $(e.srcElement).attr("item-id");
        $("#dialog-confirm").dialog({
            resizable: false,
            modal: true,
            buttons: {
                Yes: function () {
                    var token = $('input[name="__RequestVerificationToken"]').val();
                    $.ajax({
                        type: "POST",
                        url: URL_Delete,
                        data: {
                            __RequestVerificationToken: token,
                            id: id
                        },
                        cache: false,
                        dataType: "json",
                        success: function (data) {
                            if (data.Error) {
                                ShowMessage(true, data.Message)
                            }
                            else {
                                $("#btnFilter").click();
                            }
                        },
                        error: function (data) {
                            ShowMessage(true, "Unexpected error occurred. Please try again later or contact your system administrator");
                        }
                    });
                    $(this).dialog("close");
                },
                No: function () {
                    $(this).dialog("close");
                }
            }
        });
        return false;
    });

    $("#btnFilter").live("click", function (e) {
        $.ajax({
            type: "GET",
            url: URL_Filter,
            data: { TripDestination: $("#frmTripsList #TripDestination").val() },
            cache: false,
            dataType: "html",
            success: function (data) {
                $("#frmTripsList #divTripsList").html(data);
            },
            error: function (data) {
                ShowMessage(true, "Unexpected error occurred. Please try again later or contact your system administrator");
            }
        });
    });

    $("#btnClearFilter").live("click", function (e) {
        $.ajax({
            type: "GET",
            url: URL_Filter,
            data: { TripDestination: "" },
            cache: false,
            dataType: "html",
            success: function (data) {
                $("#frmTripsList #divTripsList").html(data);
            }
        });
    });

    $("#btnPrint").live("click", function (e) {
        $.ajax({
            type: "GET",
            url: URL_TravelPlan,
            data: "{}",
            cache: false,
            dataType: "html",
            success: function (data) {
                $("#TravelPlan").html(data);
                $("#TravelPlan").print();
            },
            error: function (data) {
                ShowMessage(true, "Unexpected error occurred. Please try again later or contact your system administrator");
            }
        });
    });

    jQuery.fn.print = function () {
        // NOTE: We are trimming the jQuery collection down to the
        // first element in the collection.
        if (this.size() > 1){
            this.eq( 0 ).print();
            return;
        } else if (!this.size()){
            return;
        }

        // ASSERT: At this point, we know that the current jQuery
        // collection (as defined by THIS), contains only one
        // printable element.

        // Create a random name for the print frame.
        var strFrameName = ("printer-" + (new Date()).getTime());

        // Create an iFrame with the new name.
        var jFrame = $( "<iframe name='" + strFrameName + "'>" );

        // Hide the frame (sort of) and attach to the body.
        jFrame
            .css( "width", "1px" )
            .css( "height", "1px" )
            .css( "position", "absolute" )
            .css( "left", "-9999px" )
            .appendTo( $( "body:first" ) )
        ;

        // Get a FRAMES reference to the new frame.
        var objFrame = window.frames[ strFrameName ];

        // Get a reference to the DOM in the new frame.
        var objDoc = objFrame.document;

        // Grab all the style tags and copy to the new
        // document so that we capture look and feel of
        // the current document.

        // Create a temp document DIV to hold the style tags.
        // This is the only way I could find to get the style
        // tags into IE.
        var jStyleDiv = $( "<div>" ).append(
            $( "style" ).clone()
            );

        // Write the HTML for the document. In this, we will
        // write out the HTML of the current element.
        objDoc.open();
        objDoc.write( "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">" );
        objDoc.write( "<html>" );
        objDoc.write( "<body>" );
        objDoc.write( "<head>" );
        objDoc.write( "<title>" );
        objDoc.write( document.title );
        objDoc.write( "</title>" );
        objDoc.write( jStyleDiv.html() );
        objDoc.write( "</head>" );
        objDoc.write( this.html() );
        objDoc.write( "</body>" );
        objDoc.write( "</html>" );
        objDoc.close();

        // Print the document.
        objFrame.focus();
        objFrame.print();

        // Have the frame remove itself in about a minute so that
        // we don't build up too many of these frames.
        setTimeout(
            function(){
                jFrame.remove();
            },
            (60 * 1000)
            );
    }

    ShowHideActionLinks(startupRole);
});
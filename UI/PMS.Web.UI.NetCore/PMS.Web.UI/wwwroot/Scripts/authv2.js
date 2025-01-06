
var _TOKEN_KEY = "BearerToken";
//var _LOGIN_PAGE = "/Account/Login";
//var _DEFAULT_HOMEPAGE = "/Home/Index";
//var _DHS_HOST = "http://localhost:8686/";
//var _API_URL = "http://localhost:4255/";
var currentFilterString = {};
var allUserAccess = {};

var authServices = {

    getToken: function () {
        return localStorage.getItem(_TOKEN_KEY);
    },

    setToken: function (accessToken,menuIds) {
        localStorage.setItem(_TOKEN_KEY, accessToken);
        authServices.setCookie(_TOKEN_KEY, accessToken, 365);
        if (menuIds)
            authServices.setCookie("AuthMenu", menuIds, 365);
    },

    getBearerToken: function () {
        return "Bearer " + localStorage.getItem(_TOKEN_KEY);
    },

    clearAuthData: function () {
        if (sessionStorage.getItem("MENU")) sessionStorage.removeItem("MENU");
        if (localStorage.getItem(_TOKEN_KEY)) localStorage.removeItem(_TOKEN_KEY);
        if (localStorage.getItem("UserName")) localStorage.removeItem("UserName");
        authServices.eraseCookie(_TOKEN_KEY);
        authServices.eraseCookie("AuthMenu");
    },

    setCookie: function (key, value, expiry) {
        var expires = new Date();
        expires.setTime(expires.getTime() + (expiry * 24 * 60 * 60 * 1000));
        document.cookie = key + '=' + value + ';path=/;expires=' + expires.toUTCString();
    },

    getCookie: function (key) {
        var keyValue = document.cookie.match('(^|;) ?' + key + '=([^;]*)(;|$)');
        return keyValue ? keyValue[2] : null;
    },

    eraseCookie: function (key) {
        var keyValue = authServices.getCookie(key);
        authServices.setCookie(key, keyValue, '-1');
    },
    doResetPasswordRequest: function (userName, errorFunction) {
        var result = false;
        
        
        $.ajax({
            type: "get",
            url: _API_URL + 'token/resetpasswordtoken?username=' + userName,                    
            success: function (responseData, textStatus, jqXHR) {                
                alert("Permintaan reset password diterima, silakan cek email anda");
                window.location = _DEFAULT_HOMEPAGE;
            },
            error: function (jqXHR, textStatus, errorThrown) {
                var errorMessage = "";
                if (!jqXHR.responseJSON) {
                    errorMessage = "API Server " + _API_URL + " may be unreachable, please contact the administrator";
                }
                else
                    errorMessage = jqXHR.responseJSON.Message;

                if (!errorFunction) {
                    alert(errorMessage);
                    return false;
                }
                errorFunction(errorMessage);

            }
        });
    },
    doResetPassword: function (token, newPassword, errorFunction) {
        var result = false;
        var postData = { Token: token, NewPassword: newPassword };

        $.ajax({
            type: "post",
            url: _API_URL + 'token/resetpassword',
            data: JSON.stringify(postData),
            contentType: "application/json",
            success: function (responseData, textStatus, jqXHR) {
                alert("Reset password berhasil, silakan login dengan password yang baru");
                window.location = _DEFAULT_HOMEPAGE;
            },
            error: function (jqXHR, textStatus, errorThrown) {
                var errorMessage = "";
                if (!jqXHR.responseJSON) {
                    errorMessage = "API Server " + _API_URL + " may be unreachable, please contact the administrator";
                }
                else
                    errorMessage = jqXHR.responseJSON.Message;

                if (!errorFunction) {
                    alert(errorMessage);
                    return false;
                }
                errorFunction(errorMessage);

            }
        });
    },
    doLogin: function (userName, password, returnURL, errorFunction) {
        var result = false;
        var postData = { Username: userName, Password: password };
        var accessToken = "";
        if (!returnURL || returnURL == '' || returnURL == "/")
            returnURL = _DEFAULT_HOMEPAGE;
        $.ajax({
            type: "post",
            url: _API_URL + 'token',
            data: JSON.stringify(postData),
            contentType: "application/json",
            success: function (responseData, textStatus, jqXHR) {                
                authServices.setToken(responseData);
                var decodedToken = jwt_decode(responseData);
                if (decodedToken) {
                    $('#spanUserName').html(decodedToken.fullname);
                    $('#spanUserName2').html(decodedToken.fullname);
                    if (decodedToken.chgpwd && decodedToken.chgpwd === "1") {
                        return authServices.redirectToChangePassword(returnURL);
                    }
                    
                }                
                window.location = returnURL;
            },
            error: function (jqXHR, textStatus, errorThrown) {
                var errorMessage = "";
                if (!jqXHR.responseJSON) {
                    errorMessage = "API Server " + _API_URL + " may be unreachable, please contact the administrator";
                }
                else
                    errorMessage = jqXHR.responseJSON.Message;

                if (!errorFunction) {
                    alert(errorMessage);
                    return false;
                }
                errorFunction(errorMessage);

            }
        });
    },



    kickIfNotLoggin: function (nextUrl) {
        var loginPageUrl = _LOGIN_PAGE;
        if (nextUrl)
            loginPageUrl += "?returnURL=" + nextUrl;
        else {
            loginPageUrl += "?returnURL=" + window.location.pathname;
        }


        if (!authServices.getBearerToken()) {
            authServices.clearAuthData();
            window.location = loginPageUrl;
            return;
        }

        //Validadate Token Expiration To Server & Get Menu
        $.ajax({
            type: "get",
            url: _API_URL + 'token/checktokenstatus',
            headers: { "Authorization": authServices.getBearerToken() },

            success: function (responseData, textStatus, jqXHR) {
                //if (!responseData) {
                //    authServices.clearAuthData();
                //    window.location = loginPageUrl;
                //}
                
                //$('#spanUserName').html(responseData.UserName);
                //$('#spanUserName2').html(responseData.UserName);
                authServices.setToken(responseData);
                var decodedToken = jwt_decode(responseData);
                if (decodedToken) {

                    $('#spanUserName').html(decodedToken.fullname);
                    $('#spanUserName2').html(decodedToken.fullname);
                    if (decodedToken.chgpwd && decodedToken.chgpwd === "1") {
                        if (!nextUrl)
                            nextUrl = window.location.pathname;                        
                        return authServices.redirectToChangePassword(nextUrl);
                    }
                    
                    

                }
                if (nextUrl)
                    window.location = nextUrl;
            },
            error: function (jqXHR, textStatus, errorThrown) {
                authServices.clearAuthData();
                window.location = loginPageUrl;
            }
        });
    },

    openDHSMenu: function (dhsUrl) {
        if (_DHS_HOST)
            window.location = _DHS_HOST + "Mulai/LoginByPMSToken?Token=" + authServices.getToken() + "&DHSUrl=" + dhsUrl;
        else
            alert("Link DHS Web belum dikonfigurasi dengan benar");
    },

    doLogout: function () {
        if (authServices.getBearerToken()) {
            $.ajax({
                type: "get",
                url: _API_URL + 'logout',

                headers: { "Authorization": authServices.getBearerToken() },
                success: function (responseData, textStatus, jqXHR) {
                    authServices.clearAuthData();
                    window.location = _LOGIN_PAGE;
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    authServices.clearAuthData();
                    window.location = _LOGIN_PAGE;

                }
            }).then(function () {
                if (_DHS_HOST) window.location = _DHS_HOST + "Mulai/Logout";
            });

        }
    },
    redirectToChangePassword: function (returnURL) {        
        if (this.inChangePasswordPage())
            return;

        if (!returnURL || returnURL === '' || returnURL === "/")
            returnURL = _DEFAULT_HOMEPAGE;
        else if (this.isChangePasswordPage(returnURL))
            returnURL = _DEFAULT_HOMEPAGE;
        alert("Password kadaluarsa, silakan ubah password sekarang");
        window.location = _CHANGE_PASSWORD_PAGE;
    },
    inChangePasswordPage: function () {
        return this.isChangePasswordPage(window.location.pathname.toLowerCase());
    },

    inLoginPage: function () {
        return this.isLoginPage(window.location.pathname.toLowerCase());
    },

    isChangePasswordPage: function (pathName) {
        return (pathName.toLowerCase().indexOf(_CHANGE_PASSWORD_PAGE.toLowerCase()) >= 0);
    },

    isLoginPage: function (pathName) {
        return (pathName.toLowerCase().indexOf(_LOGIN_PAGE.toLowerCase()) >= 0);
    },
    changePassword: function (oldPassword, newPassword, returnURL, errorFunction, resetToken) {
        var result = false;
        var postData = { OldPassword: oldPassword, NewPassword: newPassword };
        var accessToken = "";
        if (!returnURL || returnURL === '' || returnURL === "/")
            returnURL = _DEFAULT_HOMEPAGE;

        if (resetToken)
            resetToken = "Bearer " + resetToken;
        else
            resetToken = authServices.getBearerToken();

        $.ajax({
            type: "post",
            url: _API_URL + 'token/changepassword',
            data: JSON.stringify(postData),
            contentType: "application/json",
            headers: { "Authorization": resetToken},
            success: function (responseData, textStatus, jqXHR) {
                alert("Password berhasil diubah");
                authServices.setToken(responseData);
                window.location.pathname = returnURL;
            },
            error: function (jqXHR, textStatus, errorThrown) {
                var errorMessage = "";
                if (!jqXHR.responseJSON) {
                    errorMessage = "API Server " + _API_URL + " may be unreachable, please contact the administrator";
                }
                else
                    errorMessage = jqXHR.responseJSON.Message;

                if (!errorFunction) {
                    alert(errorMessage);
                    return false;
                }
                errorFunction(errorMessage);

            }
        });
    }

}
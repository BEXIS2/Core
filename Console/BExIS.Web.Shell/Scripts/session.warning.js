$(document).ready(function () {
    //delete loggedout session cookie
    delete_cookie('loggedout', window.location.hostname);

    initSessionMonitor();
    awakeServerSession();

    getLastActivityTime = true;
});

//Load jQuery First
//How frequently to check for session expiration in milliseconds
//10000 - every 10 sec
var sess_pollInterval = 10000;
//How many minutes the session is valid for
//declared and assigned value in globaljs
//var global_sess_expirationSeconds = 30;
//How many minutes before the warning prompt
//var sess_warningSeconds = 120;

var sess_intervalID;
var CountDownTimerID;
var sess_lastActivity;
var actualCheckingValue = (global_sess_expirationSeconds - (global_sess_warningSeconds + 60));
var getLastActivityTime;
//60 seconds for expected network lag time

var countDownStartesFrom = 1;

function initSessionMonitor() {
    sess_lastActivity = new Date();
    setCookie();
    sessSetInterval();
    $(document).bind('keypress', function (ed, e) { sessKeyPressed(ed, e); });
    $(document).bind('mousemove', function (ed, e) { sessKeyPressed(ed, e); });
    $(document).bind('click', function (ed, e) { sessKeyPressed(ed, e); });
}
function sessSetInterval() {
    sess_intervalID = setInterval('sessInterval()', sess_pollInterval);
}
function sessClearInterval() {
    clearInterval(sess_intervalID);
}
function sessKeyPressed(ed, e) {
    setTimeout(function () {
        //msg('getLastActivityTime -', getLastActivityTime);

        if (getLastActivityTime == true) {
            sess_lastActivity = new Date();
        }
    }, 0);
}
function sessLogOut() {
    window.location.href = global_LogoutUrl;
    deleteCookie();
    deleteSession();
}
function ResetServerSession() {
    var oldgetLastActivityTime = getLastActivityTime;

    $.ajax({ url: global_sess_resetPage, type: "GET", cache: false, async: false });

    //reset old value after ajax call
    getLastActivityTime = oldgetLastActivityTime;
}

function sessInterval() {
    var diffSec = GetTimeDifference();
    //if session timeout value is 1200 sec then warning should start when the time diff is (1200-120-60)sec and will start a timer for 120 sec after that it will get automatically logout, before 60 sec of actual session timeout
    if (diffSec >= actualCheckingValue) {
        //wran before expiring
        //stop the timer
        sessClearInterval();
        //promt for attention
        countDownStartesFrom = global_sess_warningSeconds;
        var msg = '<div id="divTimeout"><div class="popup-block">Your session will expire in <div id="divCountDown">' + countDownStartesFrom + '  Sec</div>, click "Continue" to keep working or "Logout" if you are finished. <input type="button" class="btn btn-active" value="Continue" onclick="CancelLogout();"> <input type="button" class="btn btn-active" value="Logout" onclick="sessLogOut();"> </div></div><div class="backdrop"></div>'
        $("body").prepend(msg);
        $("body").addClass("opened");
        $("html").addClass("opened");
        $("#divTimeout").show();
        $(".backdrop").show();

        //stop getting latest activity time
        getLastActivityTime = false;

        CountDownTimerID = setInterval('CountDown()', 1000);
        //reset server session here
        ResetServerSession();
        clearInterval(actualsess_intervalID);
    }
}

function CancelLogout() {
    var diffSec = GetTimeDifference();

    $("#divTimeout").remove();
    $(".backdrop").remove();
    $("body").removeClass("opened");
    $("html").removeClass("opened");

    clearInterval(CountDownTimerID);

    if (diffSec > global_sess_expirationSeconds) {
        //timed out
        sessLogOut();
    }
    else {
        //reset inactivity timer
        sessSetInterval();
        sess_lastActivity = new Date();
        //msg('CancelLogout', sess_lastActivity);
        //reset server session here
        ResetServerSession();

        //rerun this function to awake server session timeout
        awakeServerSession();
        getLastActivityTime = true;
    }
}

function GetTimeDifference() {
    //if last activity is lesser than cookie's value then set the cookie value to last activity time
    var lastActivityFromCookie = getCookie();

    if (sess_lastActivity < lastActivityFromCookie) {
        sess_lastActivity = lastActivityFromCookie;
    }

    var diff = (new Date() - sess_lastActivity);
    var diffSec = (diff / 1000);
    return diffSec;
}

function CountDown() {
    //scenario- when 2 screens opened in 2 tabs, in one screen start showing warning box at that same time user is working in another screen
    //then cookie's lastupdated date will be updated by current screen so get that value and check if sess_lastActivity's date is lesser than that value
    //if so that means variable contains old value but last activity performed just now. In this scenario hide the warning box and cancel this countdown
    var lastActivityFromCookie = getCookie();
    //msg('getLastActivityTime - ', getLastActivityTime);
    //msg('lastActivityFromCookie - ', lastActivityFromCookie);
    //msg('sess_lastActivity - ', sess_lastActivity);

    if (lastActivityFromCookie > sess_lastActivity) {
        sess_lastActivity = lastActivityFromCookie;
        //stop timer hide msg box
        CancelLogout();
    }
    else {
        countDownStartesFrom = countDownStartesFrom - 1;

        if (countDownStartesFrom <= 0) {
            clearInterval(CountDownTimerID);
            sessLogOut();
        }
        else {
            $("#divCountDown").text(countDownStartesFrom + '  Sec');
        }
    }
}

//scenario: if a user open a form page and spend a long time to fill the form then from client side session will be active as he is doing something on the page
//but from server side application session will be timed out
//prevention: we can start a timer at page load whose tick time will be (session timeout value-60) sec. then before actual session timeout timer can
//call a function which can call ajax to rejuvenate server side session. Considering 60sec as lag time

var actualsess_intervalID;
//this function will initiate a timer which will execute every (sessionTimeOut-60)*1000 millisec
function awakeServerSession() {
    actualsess_intervalID = setInterval('ResetServerSession()', (global_sess_expirationSeconds - 60) * 1000);
}

function setCookie() {
    //in every 2 sec get the last activity time and store that in cookie
    setInterval(function () {
        deleteCookie();
        set_cookie('lastupdated', sess_lastActivity.toString(), 1, window.location.hostname);
    }, 2000)
}

function getCookie() {
    //when timed out from other tab and logged out there then force logout
    if (get_cookie('loggedout') == 'true') {
        sessLogOut();
    }
    else {
        return new Date(get_cookie('lastupdated'));
    }
}

function deleteCookie() {
    delete_cookie('lastupdated', window.location.hostname);
}

//to be called from application's logout button click
function deleteSession() {
    set_cookie('loggedout', 'true', 1, window.location.hostname);
}

//Generic cookie access functions
function set_cookie(cookie_name, cookie_value, lifespan_in_days, valid_domain) {
    setTimeout(function () {
        // http://www.thesitewizard.com/javascripts/cookies.shtml
        var domain_string = valid_domain ? ("; domain=" + valid_domain) : '';
        document.cookie = cookie_name + "=" + encodeURIComponent(cookie_value) +
            "; max-age=" + 60 * 60 * 24 * lifespan_in_days +
            "; path=/" + domain_string;
    }, 0);
}

function get_cookie(name) {
    //http://stackoverflow.com/questions/10730362/get-cookie-by-name
    var value = "; " + document.cookie;
    var parts = value.split("; " + name + "=");
    if (parts.length == 2) return decodeURIComponent(parts.pop().split(";").shift());
}

function delete_cookie(cookie_name, valid_domain) {
    // http://www.thesitewizard.com/javascripts/cookies.shtml
    var domain_string = valid_domain ? ("; domain=" + valid_domain) : '';
    document.cookie = cookie_name + "=; max-age=0; path=/" + domain_string;
}

function msg(prefix, val) {
    console.log(prefix + '- ' + val.toString());
}
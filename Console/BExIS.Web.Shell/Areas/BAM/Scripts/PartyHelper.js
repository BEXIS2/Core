function parsePartyCondition(condition) {
    var newCondition = condition;
    //if text is sourounded with [] means the value comes from an element
    //Extract such text and replace them by the value of the related element
    var re = /\[(.*?)\]/g;
    var m;
    do {
        m = re.exec(condition);
        if (m) {
            var element = $("[AttributeName='" + m[1].toLowerCase() + "']");
            elementVal = element.val();
            //TODO: more than one element is radio list but it might be different in future
            if (element.length > 1)
                elementVal = $("[AttributeName='" + m[1].toLowerCase() + "']:checked").val();
            newCondition = newCondition.replace(m[0], "'" + elementVal + "'");
        }
    } while (m)
    return newCondition;
}
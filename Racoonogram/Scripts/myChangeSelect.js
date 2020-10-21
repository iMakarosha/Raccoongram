{
    $("li.selectTypeLi").click(function (e) {
        $("#submit-order").val(this.id);
        $("#selectType").html($(this).html() + " <span class='caret'></span>");
    });
}
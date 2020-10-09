{
    $("li.selectTypeLi").click(function (e) {
        $("#submit-order").val(this.id);
        console.log(this.id);
        console.log($("#submit-order").val());
        $("#selectType").html($(this).html() + " <span class='caret'></span>");
    });
}
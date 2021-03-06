﻿{
    function handleChange(evt) {
        var file = evt.target.files; // FileList object
        console.log(file);
        var f = file[0];
    // Only process image files.
        if (!f.type.match('image.*')) {
            alert("Image only please....");
        }

        var reader = new FileReader();
        document.getElementById("logo_file_ContentType").val = f.name;
        document.getElementById("logo_file_ContentLength").val = f.size;
        document.getElementById("logo_file_FileName").val = f.name;
        console.log(document.getElementById("logo_file_ContentType").val);
        console.log(document.getElementById("logo_file_ContentLength").val);
        console.log(document.getElementById("logo_file_FileName").val);
    // Closure to capture the file information.
    reader.onload = (function(theFile) {
        return function(e) {
            // Render thumbnail.
            document.getElementById('logo-img').src = e.target.result;
                $s = document.getElementById('logo-img').width;
                $('#logo-img').css("height", $s)
        };
    })(f);
    // Read in the image file as a data URL.
        reader.readAsDataURL(f);

}
    document.getElementById('logo_file').addEventListener('change', handleChange, false);
}

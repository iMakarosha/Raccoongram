﻿{
    $("#editModal").on("show.bs.modal", function (event) {
        var button = $(event.relatedTarget);
        var rec = button.data('whatever');
        var modal = $(this);
        modal.find('.modal-body #imneed').val(rec);
        $("#partForm").submit();
    });

    var deleteIndex = 0;

    function deletePhoto(ImId) {
            var data = new FormData();
            data.append("imid", ImId);
            console.log(ImId);
            $.ajax({
                type: "POST",
                url: '/Manage/DeletePhoto',
                contentType: false,
                processData: false,
                data: data,
                success: function (result) {
                    location.reload();
                },
                error: function (xhr, status, p3) {
                }
            });
        }

    $("#clear-all").click(function () {
        clear();
        $("form").trigger('reset');
    });
    function clear() {
        document.getElementById("block-errors").innerHTML = "";
        document.getElementById("list").innerHTML = "";
        document.getElementById("listOfErr").innerHTML = "";

    };

    function recheckI(imageI) {
        if (imageI.naturalHeight < 1920 && imageI.naturalWidth < 1920) {
            document.getElementById(imageI.id).parentElement.remove();
            li = document.createElement("li");
            li.innerHTML = "Нельзя загрузить изображение " + Store.files[deleteIndex].name +
                ", так как его ширина и высота меньше 1920 пикселей.";
            document.getElementById("listOfErr").appendChild(li);
            removeFile(deleteIndex);
            $("form").trigger('reset');
        }
        else {
            var data = new FormData();
            console.log("recheckI");
            var file = Store.files[0];
            console.log(file.filename);
            if (file.type === "image/jpeg") {
                data.append("file0", file);
            }
            $.ajax({
                type: "POST",
                url: '/Manage/CheckImage',
                contentType: false,
                data: data,
                processData: false,
                success: function (result) {
                    console.log(result);
                    var arrayJson = JSON.parse(result);
                    if (arrayJson["metadata"]["format"] != "Jpeg") {
                        alert("Только файлы .jpg!");
                    } else if (arrayJson["adult"]["isAdultContent"]==true) {
                        alert("Контент возрастной категории 18+ запрещено выгружать в Raccoonogram");
                        document.getElementById(imageI.id).parentElement.remove();
                        li = document.createElement("li");
                        li.innerHTML = "Контент возрастной категории 18+ запрещено выгружать в Raccoonogram.";
                        document.getElementById("listOfErr").appendChild(li);
                        removeFile(deleteIndex);
                    }
                    else {
                        console.log(arrayJson);
                        $categories = ""; $descr = "";
                        var sd = arrayJson["categories"];
                        sd.forEach(function (item, i, sd) {
                            $categories += item["name"] + ", ";
                        });
                        document.getElementById("Category").value = ($categories.replace(/_/g, " ")).substring(0, $categories.length - 2);
                        document.getElementById("KeyWords").value = arrayJson["description"]["tags"].join(' ');

                        var sdf = arrayJson["description"]["captions"];
                        sdf.forEach(function (item, i, sd) {
                            $descr += item["text"] + ", ";
                        });
                        document.getElementById("Description").value = $descr.substring(0, $descr.length - 2);
                    }
                },
                error: function (xhr, status, p3) {
                    console.log(xhr);
                    console.log(status);
                    console.log(p3);

                }
            });
        }
    };

    var Store = {files: [] } /* какое-то хранищие файлов, для примера так*/
    document.getElementById('fileInput').addEventListener('change', handleChange, false);

    // при выборе файлов, мы будем их добавлять
    function handleChange(evt) {
        clear();
        Store.files.splice(0, Store.files.length);
        var file = evt.target.files; // FileList object
        console.log(file);
        var f = file[0];
        addFiles(f);
        // Only process image files.
        if (!f.type.match('image.*')) {
            alert("Image only please....");
        }

        var reader = new FileReader();
        reader.onload = (function (theFile) {
            return function (e) {
                // Render thumbnail.
                li = document.createElement("div");
                li.innerHTML = "<img id='im-client' onload='recheckI(this);' src='" + e.target.result + "' />";
                document.getElementById('list').appendChild(li);
            };
        })(f);
        // Read in the image file as a data URL.
        reader.readAsDataURL(f);
        console.log(f);
    }

    function addFiles(files) {
        Store.files = Store.files.concat(files);
    }

    function removeFile(index) {
        Store.files.splice(index, 1);
    }

    $(".text-box").change(function () {
        document.getElementById("block-errors").innerHTML = "";
    });

    $("#fileInput").change(function () {
        document.getElementById("block-errors").innerHTML = "";
        document.getElementById("list").innerHTML = "";
    });

    $('#submitFileInput').on('click', function (e) {
        e.preventDefault();
        var files = Store.files;
        if (files.length > 0) {
            if (window.FormData !== undefined) {
                var data = new FormData();
                //console.log(data);

                for (var x = 0; x < files.length; x++) {
                    if (files[x].type === "image/jpeg") {
                        data.append("file" + x, files[x]);
                    }
                }

                //console.log(data);
                data.append("Category", document.getElementById("Category").value);
                data.append("KeyWords", document.getElementById("KeyWords").value);
                data.append("Price", document.getElementById("Price").value);
                data.append("Description", document.getElementById("Description").value);

                $.ajax({
                    type: "POST",
                    url: '/Manage/Upload',
                    contentType: false,
                    processData: false,
                    data: data,
                    success: function (result) {
                        if (result === "Изображения успешно загружены") {
                            alert(result);
                            location.reload();
                        }
                        else
                        document.getElementById("block-errors").innerHTML = result;
                    },
                    error: function (xhr, status, p3) {
                        if (xhr.status === "500") {
                            document.getElementById("block-errors").innerHTML = "Размер загружаемых изображений превысил установленный лимит в 12МБ";
                        }
                    }
                });
            } else {
            alert("Браузер не поддерживает загрузку файлов html5!");
            }
        }
    });
}

window.addEventListener('load', function () {
    proyectosHoy();
});

var tareas = "";

function proyectosHoy() {
    $.ajax({
        url: "Proyecto/ProyectosHoy",
        type: "GET",
        dataType: "json",
        success: function (data) {
            if (data.length > 0) {

                for (var i = 0; i < data.length; i++) {
                    var nomProyecto = data[i].Item3;
                    var nomFase = data[i].Item4;
                    var jefeProyecto = (data[i].Item2 == null ? " sin técnico" : (data[i].Item2 === "" ? "" : data[i].Item2));

                    if (jefeProyecto === "") {
                        tareas = tareas + "La tarea " + nomProyecto + " en fase " + nomFase + " termina hoy.\n";
                       
                    } else if (jefeProyecto === " sin técnico") {
                        tareas = tareas + "La tarea " + nomProyecto + jefeProyecto + " en fase " + nomFase + " termina hoy.\n";
                       
                    } else {
                        tareas = tareas + "La tarea " + nomProyecto + " de " + jefeProyecto + " en fase " + nomFase + " termina hoy.\n";
                       
                    }
                }
                renderizarNotificacion(tareas);
                tareas = "";
            }
        }, contentType: 'application/json'
    });
}

function renderizarNotificacion(message) {

    message = message.replace(/\n/g, "<br>");

    var notification = $("<div>");
    notification.html(message);
    notification.css({
        "position": "fixed",
        "top": "80px",
        "right": "20px",
        "padding": "10px",
        "background-color": "#FE744F",
        "color": "#fff",
        "font-size": "16px",
        "border-radius": "4px",
        "box-shadow": "0 2px 10px rgba(0,0,0,0.7)",
        "z-index": "9999",
        "width":"40%",
    });
    $("nav").append(notification);
    notification.fadeIn().delay(10000).fadeOut(function () {
        notification.remove();
    });
}
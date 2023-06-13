const eliminarToast = () =>
    Toastify({
        text: "Usuario eliminado",
        duration: 1500,
        gravity: "bottom",
        position: "center",
        stopOnFocus: true,
        style: {
            background: "#E14848",
        },
    }).showToast();

const registrarToast = () =>
    Toastify({
        text: "Usuario registrado",
        duration: 1500,
        gravity: "top",
        position: "center",
        stopOnFocus: true,
        style: {
            background: "linear-gradient(to right, #00b09b, #96c93d)",
        },
        
    }).showToast();

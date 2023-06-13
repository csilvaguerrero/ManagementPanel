
//Script Comprobar nombre usuario valido en Registrar

console.log("Script cargado");

const myFieldE = document.querySelector('#EquipoControlar');
const myFieldErrorE = document.querySelector('#errore');

myFieldE.addEventListener('blur', function () {
    const value = myFieldE.value;
    fetch(`/Equipo/CheckUnique?value=${value}`)
        .then(function (response) {
            return response.json();
        })
        .then(function (isUnique) {
            if (!isUnique) {
                myFieldE.classList.add('is-invalid');
                myFieldE.value = null;
                myFieldErrorE.textContent = 'Este nombre de equipo ya está en uso';
            } else {
                myFieldE.classList.remove('is-invalid');
                myFieldErrorE.textContent = '';
            }
        });
});
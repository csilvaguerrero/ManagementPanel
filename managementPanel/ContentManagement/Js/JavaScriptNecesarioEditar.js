
//Script Comprobar nombre usuario valido en Registrar

console.log("Script cargado");
const myFieldE2 = document.querySelector('#EquipoControlar2');
const myFieldErrorE2 = document.querySelector('#errore2');

myFieldE2.addEventListener('blur', function () {
    const value = myFieldE2.value;
    fetch(`/Equipo/CheckUnique?value=${value}`)
        .then(function (response) {
            return response.json();
        })
        .then(function (isUnique) {
            if (!isUnique) {
                myFieldE2.classList.add('is-invalid');
                myFieldE2.value = null;
                myFieldErrorE2.textContent = 'Este nombre de equipo ya está en uso';
            } else {
                myFieldE2.classList.remove('is-invalid');
                myFieldErrorE2.textContent = '';
            }
        });
});
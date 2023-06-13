//Script Comprobar nombre usuario valido en crear proyecto

const myFieldP = document.querySelector('#ProyectoControlar');
const myFieldErrorP = document.querySelector('#errorp');

myFieldP.addEventListener('blur', function () {
    const value = myFieldP.value;
    fetch(`/Proyecto/CheckUnique?value=${value}`)
        .then(function (response) {
            return response.json();
        })
        .then(function (isUnique) {
            if (!isUnique) {
                myFieldP.classList.add('is-invalid');
                myFieldP.value = null;
                myFieldErrorP.textContent = 'Este código de proyecto ya está en uso';
            } else {
                myFieldP.classList.remove('is-invalid');
                myFieldErrorP.textContent = '';
            }
        });
});

console.log("PROJECT");

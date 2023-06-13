
//Script Comprobar nombre usuario valido en Registrar
const myField = document.querySelector('#UsuarioControlar');
const myFieldError = document.querySelector('#error');

myField.addEventListener('blur', function () {
    const value = myField.value;
    fetch(`/Login/CheckUnique?value=${value}`)
        .then(function (response) {
            return response.json();
        })
        .then(function (isUnique) {
            if (!isUnique) {
                myField.classList.add('is-invalid');
                myField.value = null;
                myFieldError.textContent = 'Este nombre de usuario ya está en uso';
            } else {
                myField.classList.remove('is-invalid');
                myFieldError.textContent = '';
            }
        });
});


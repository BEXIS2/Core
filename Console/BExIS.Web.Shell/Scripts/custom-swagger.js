(function () {
    $(function () {
        $('#input_apiKey').attr('placeholder', 'JSON Web Token');
        $('#input_apiKey').off();
        $('#input_apiKey').on('change', function () {
            var key = this.value;
            console.info('Set bearer token to: ' + key);
            if (key && key.trim() !== '') {
                swaggerUi.api.clientAuthorizations.add("api_key", new SwaggerClient.ApiKeyAuthorization("Authorization", "Bearer " + key, "header"));
            }
        });
    });
})();
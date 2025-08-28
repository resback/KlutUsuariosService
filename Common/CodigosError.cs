namespace UsuariosAuth.Common
{
    public static class CodigosError
    {
        public const string CredencialesInvalidas = "AUTH_001";
        public const string CuentaBloqueada = "AUTH_002";
        public const string TokenInvalido = "AUTH_003";
        public const string TokenComprometido = "AUTH_004";
        public const string RecursoNoEncontrado = "GEN_404";
        public const string ErrorInterno = "GEN_500";
        public const string Validacion = "GEN_400";
    }
}


namespace Thinktecture.IdentityModel.Tokens.Http
{
    public enum ClientCertificateMode
    {
        ChainValidation,
        PeerValidation,
        ChainValidationWithIssuerSubjectName,
        ChainValidationWithIssuerThumbprint,
        IssuerThumbprint
    }
}

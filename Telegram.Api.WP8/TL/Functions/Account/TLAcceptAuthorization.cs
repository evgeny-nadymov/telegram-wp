namespace Telegram.Api.TL.Functions.Account
{
    class TLAcceptAuthorization : TLObject
    {
        public const uint Signature = 0xe7027c94;

        public TLInt BotId { get; set; }

        public TLString Scope { get; set; }

        public TLString PublicKey { get; set; }

        public TLVector<TLSecureValueHash> ValueHashes { get; set; }

        public TLSecureCredentialsEncrypted Credentials { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                BotId.ToBytes(),
                Scope.ToBytes(),
                PublicKey.ToBytes(),
                ValueHashes.ToBytes(),
                Credentials.ToBytes());
        }
    }
}

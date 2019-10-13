// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.IO;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    public enum GameFlags
    {
        Document = 0x1
    }

    public class TLGame : TLObject
    {
        public const uint Signature = TLConstructors.TLGame;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLLong Id { get; set; }

        public TLLong AccessHash { get; set; }

        public TLString ShortName { get; set; }

        public TLString Title { get; set; }

        public TLString Description { get; set; }

        public TLPhotoBase Photo { get; set; }

        private TLDocumentBase _document;

        public TLDocumentBase Document
        {
            get { return _document; }
            set { SetField(out _document, value, ref _flags, (int) GameFlags.Document); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLLong>(bytes, ref position);
            AccessHash = GetObject<TLLong>(bytes, ref position);
            ShortName = GetObject<TLString>(bytes, ref position);
            Title = GetObject<TLString>(bytes, ref position);
            Description = GetObject<TLString>(bytes, ref position);
            Photo = GetObject<TLPhotoBase>(bytes, ref position);
            Document = GetObject<TLDocumentBase>(Flags, (int) GameFlags.Document, null, bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Id.ToBytes(),
                AccessHash.ToBytes(),
                ShortName.ToBytes(),
                Title.ToBytes(),
                Description.ToBytes(),
                Photo.ToBytes(),
                ToBytes(Document, Flags, (int) GameFlags.Document));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Id = GetObject<TLLong>(input);
            AccessHash = GetObject<TLLong>(input);
            ShortName = GetObject<TLString>(input);
            Title = GetObject<TLString>(input);
            Description = GetObject<TLString>(input);

            Photo = GetObject<TLPhotoBase>(input);
            Document = GetObject<TLDocumentBase>(Flags, (int) GameFlags.Document, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Id.ToStream(output);
            AccessHash.ToStream(output);
            ShortName.ToStream(output);
            Title.ToStream(output);
            Description.ToStream(output);
            Photo.ToStream(output);
            ToStream(output, Document, Flags, (int) GameFlags.Document);
        }

        public override string ToString()
        {
            return string.Format("TLGame id={0} short_name={1} title={2} description={3} photo={4} document={5}", Id, ShortName, Title, Description, Photo, Document);
        }
    }
}

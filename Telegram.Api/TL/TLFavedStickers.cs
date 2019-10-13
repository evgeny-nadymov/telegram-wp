// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.IO;
using System.Linq;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    public abstract class TLFavedStickersBase : TLObject { }

    public class TLFavedStickersNotModified : TLFavedStickersBase
    {
        public const uint Signature = TLConstructors.TLFavedStickersNotModified;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }
    }

    public class TLFavedStickers : TLFavedStickersBase
    {
        public const uint Signature = TLConstructors.TLFavedStickers;

        public virtual TLInt Hash { get; set; }

        public TLVector<TLStickerPack> Packs { get; set; }

        public TLVector<TLDocumentBase> Documents { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Hash = GetObject<TLInt>(bytes, ref position);
            Packs = GetObject<TLVector<TLStickerPack>>(bytes, ref position);
            Documents = GetObject<TLVector<TLDocumentBase>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Hash.ToBytes(),
                Packs.ToBytes(),
                Documents.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Hash = GetObject<TLInt>(input);
            Packs = GetObject<TLVector<TLStickerPack>>(input);
            Documents = GetObject<TLVector<TLDocumentBase>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Hash.ToStream(output);
            Packs.ToStream(output);
            Documents.ToStream(output);
        }

        public void AddSticker(TLDocumentBase document)
        {
            Documents.Insert(0, document);
            Hash = TLUtils.GetFavedStickersHash(Documents);

            var document54 = document as TLDocument54;
            if (document54 != null)
            {
                var emoticon = document54.Emoticon;
                if (!string.IsNullOrEmpty(emoticon))
                {
                    var added = false;
                    for (var i = 0; i < Packs.Count; i++)
                    {
                        if (Packs[i].Emoticon.ToString() == emoticon)
                        {
                            var item = Packs[i].Documents.FirstOrDefault(x => x.Value == document54.Index);
                            if (item == null)
                            {
                                Packs[i].Documents.Insert(0, document54.Id);
                                added = true;
                                break;
                            }
                        }
                    }
                    if (!added)
                    {
                        Packs.Insert(0, new TLStickerPack{ Emoticon = new TLString(emoticon), Documents = new TLVector<TLLong>{ document54.Id }});
                    }
                }
            }
        }

        public void RemoveSticker(TLDocumentBase document)
        {
            for (var i = 0; i < Documents.Count; i++)
            {
                if (Documents[i].Index == document.Index)
                {
                    Documents.RemoveAt(i);
                    break;
                }
            }

            Hash = TLUtils.GetFavedStickersHash(Documents);

            var document54 = document as TLDocument54;
            if (document54 != null)
            {
                var emoticon = document54.Emoticon;
                if (!string.IsNullOrEmpty(emoticon))
                {
                    for (var i = 0; i < Packs.Count; i++)
                    {
                        if (Packs[i].Emoticon.ToString() == emoticon)
                        {
                            for (int j = 0; j < Packs[i].Documents.Count; j++)
                            {
                                if (Packs[i].Documents[j].Value == document54.Index)
                                {
                                    Packs[i].Documents.RemoveAt(j);
                                    break;
                                }
                            }

                            if (Packs[i].Documents.Count == 0)
                            {
                                Packs.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}

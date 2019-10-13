// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Api.TL;

namespace TelegramClient.Views.Controls
{
    public class GroupedMessagePosition
    {
        public float AspectRatio { get; set; }
        public bool IsEdge { get; set; }
        public int Flags { get; private set; }
        public bool IsLast { get; set; }
        public int LeftSpanOffset { get; set; }
        public byte MaxX { get; private set; }
        public byte MaxY { get; private set; }
        public byte MinX { get; private set; }
        public byte MinY { get; private set; }
        public float Height { get; private set; }
        public int Width { get; set; }
        public float[] SiblingHeights { get; set; }
        public int SpanSize { get; set; }

        public void Set(int minX, int maxX, int minY, int maxY, int w, float h, int flags)
        {
            MinX = (byte)minX;
            MaxX = (byte)maxX;
            MinY = (byte)minY;
            MaxY = (byte)maxY;
            SpanSize = w;
            Width = w;
            Height = h;
            Flags = (byte)flags;
        }

        public override string ToString()
        {
            return
                string.Format(
                    "AspectRatio={0} IsEdge={1} Flags={2} IsLast={3} LeftSpanOffset={4} MaxX={5} MaxY={6} MinX={7} MinY={8} Height={9} Width={10} SiblingHeights=[{11}] SpanSize={12}",
                    AspectRatio, IsEdge, GetFlagsString(Flags), IsLast, LeftSpanOffset, MaxX, MaxY, MinX, MinY, Height, Width, string.Join(",", SiblingHeights ?? new float[] { }), SpanSize);
        }

        private string GetFlagsString(int flags)
        {
            var sb = new StringBuilder();
            if ((flags & GroupedMessages.POSITION_FLAG_LEFT) == GroupedMessages.POSITION_FLAG_LEFT) sb.Append("l ");
            if ((flags & GroupedMessages.POSITION_FLAG_RIGHT) == GroupedMessages.POSITION_FLAG_RIGHT) sb.Append("r ");
            if ((flags & GroupedMessages.POSITION_FLAG_TOP) == GroupedMessages.POSITION_FLAG_TOP) sb.Append("t ");
            if ((flags & GroupedMessages.POSITION_FLAG_BOTTOM) == GroupedMessages.POSITION_FLAG_BOTTOM) sb.Append("b ");

            return sb.ToString();
        }
    }

    public class GroupedMessages
    {
        public const int POSITION_FLAG_LEFT = 1;
        public const int POSITION_FLAG_RIGHT = 2;
        public const int POSITION_FLAG_TOP = 4;
        public const int POSITION_FLAG_BOTTOM = 8;

        private int _maxSizeWidth = 800;
        public int MaxSizeWidth
        {
            get { return _maxSizeWidth; }
        }

        private List<GroupedMessagePosition> _posArray = new List<GroupedMessagePosition>();

        public long GroupedId { get; set; }
        public bool HasSibling { get; private set; }

        public float Height { get; private set; }
        public int Width { get; private set; }

        public List<TLObject> Messages = new List<TLObject>();
        public Dictionary<TLObject, GroupedMessagePosition> Positions = new Dictionary<TLObject, GroupedMessagePosition>();
        private float _scale;

        private class MessageGroupedLayoutAttempt
        {
            public float[] Heights { get; private set; }
            public int[] LineCounts { get; private set; }

            public MessageGroupedLayoutAttempt(int i1, int i2, float f1, float f2)
            {
                LineCounts = new int[] { i1, i2 };
                Heights = new float[] { f1, f2 };
            }

            public MessageGroupedLayoutAttempt(int i1, int i2, int i3, float f1, float f2, float f3)
            {
                LineCounts = new int[] { i1, i2, i3 };
                Heights = new float[] { f1, f2, f3 };
            }

            public MessageGroupedLayoutAttempt(int i1, int i2, int i3, int i4, float f1, float f2, float f3, float f4)
            {
                LineCounts = new int[] { i1, i2, i3, i4 };
                Heights = new float[] { f1, f2, f3, f4 };
            }
        }

        private float MultiHeight(float[] array, int start, int end)
        {
            float sum = 0.0f;
            for (int a = start; a < end; a++)
            {
                sum += array[a];
            }

            return 800.0f * _scale / sum;
        }

        public void Calculate(int desiredWidth)
        {
            _posArray.Clear();
            Positions.Clear();
            int count = Messages.Count;
            if (count <= 1)
            {
                return;
            }

            int totalWidth = 0;
            float totalHeight = 0.0f;

            _scale = (float)desiredWidth / _maxSizeWidth;
            _maxSizeWidth = desiredWidth;
            int firstSpanAdditionalSize = (int)(200 * _scale);
            int minHeight = (int)(120 * _scale);
            int minWidth = (int)(96 * _scale);
            int paddingsWidth = (int)(32 * _scale);
            float maxSizeHeight = 814.0f * _scale;
            StringBuilder proportions = new StringBuilder();
            float averageAspectRatio = 1.0f;
            bool isOut = false;
            int maxX = 0;
            bool forceCalc = false;

            for (int a = 0; a < count; a++)
            {
                int w = 0;
                int h = 0;
                var decryptedMessageObject = Messages[a] as TLDecryptedMessage;
                if (decryptedMessageObject != null)
                {
                    TLVector<TLPhotoSizeBase> photoThumbs = null;
                    var photoMedia = decryptedMessageObject.Media as TLDecryptedMessageMediaPhoto;
                    var documentMedia = decryptedMessageObject.Media as TLDecryptedMessageMediaDocument;
                    var videoMedia = decryptedMessageObject.Media as TLDecryptedMessageMediaVideo;
                    if (photoMedia != null)
                    {
                        photoThumbs = new TLVector<TLPhotoSizeBase> { new TLPhotoSize { W = photoMedia.W, H = photoMedia.H } };
                    }
                    else if (documentMedia != null)
                    {
                        photoThumbs = new TLVector<TLPhotoSizeBase> { new TLPhotoSize { W = documentMedia.ThumbW.Value > 0 ? documentMedia.ThumbW : new TLInt(90), H = documentMedia.ThumbH.Value > 0 ? documentMedia.ThumbH : new TLInt(90) } };
                    }
                    else if (videoMedia != null)
                    {
                        photoThumbs = new TLVector<TLPhotoSizeBase> { new TLPhotoSize { W = videoMedia.W.Value > 0 ? videoMedia.W : new TLInt(90), H = videoMedia.H.Value > 0 ? videoMedia.H : new TLInt(90) } };
                    }
                    TLPhotoSizeBase photoSize = GetClosestPhotoSizeWithSize(photoThumbs, 1280);
                    var size = photoSize as TLPhotoSize;
                    var cachedSize = photoSize as TLPhotoCachedSize;
                    if (size != null)
                    {
                        w = size.W.Value;
                        h = size.H.Value;
                    }
                    else if (cachedSize != null)
                    {
                        w = cachedSize.W.Value;
                        h = cachedSize.H.Value;
                    }
                    GroupedMessagePosition position = new GroupedMessagePosition();
                    position.IsLast = a == count - 1;
                    position.AspectRatio = w / (float)h;

                    if (position.AspectRatio > 1.2f)
                    {
                        proportions.Append("w");
                    }
                    else if (position.AspectRatio < 0.8f)
                    {
                        proportions.Append("n");
                    }
                    else
                    {
                        proportions.Append("q");
                    }

                    averageAspectRatio += position.AspectRatio;

                    if (position.AspectRatio > 2.0f)
                    {
                        forceCalc = true;
                    }

                    Positions[decryptedMessageObject] = position;
                    _posArray.Add(position);
                }

                var messageObject = Messages[a] as TLMessage;
                if (messageObject != null)
                {
                    TLVector<TLPhotoSizeBase> photoThumbs = null;
                    var photoMedia = messageObject.Media as TLMessageMediaPhoto;
                    var documentMedia = messageObject.Media as TLMessageMediaDocument;
                    if (photoMedia != null)
                    {
                        var photo = photoMedia.Photo as TLPhoto;
                        if (photo != null)
                        {
                            photoThumbs = photo.Sizes;
                        }
                    }
                    else if (documentMedia != null)
                    {
                        var document = documentMedia.Document as TLDocument;
                        if (document != null)
                        {
                            photoThumbs = new TLVector<TLPhotoSizeBase> { document.Thumb };
                        }
                    }
                    TLPhotoSizeBase photoSize = GetClosestPhotoSizeWithSize(photoThumbs, 1280);
                    var size = photoSize as TLPhotoSize;
                    var cachedSize = photoSize as TLPhotoCachedSize;
                    if (size != null)
                    {
                        w = size.W.Value;
                        h = size.H.Value;
                    }
                    else if (cachedSize != null)
                    {
                        w = cachedSize.W.Value;
                        h = cachedSize.H.Value;
                    }
                    GroupedMessagePosition position = new GroupedMessagePosition();
                    position.IsLast = a == count - 1;
                    position.AspectRatio = w / (float)h;

                    if (position.AspectRatio > 1.2f)
                    {
                        proportions.Append("w");
                    }
                    else if (position.AspectRatio < 0.8f)
                    {
                        proportions.Append("n");
                    }
                    else
                    {
                        proportions.Append("q");
                    }

                    averageAspectRatio += position.AspectRatio;

                    if (position.AspectRatio > 2.0f)
                    {
                        forceCalc = true;
                    }

                    Positions[messageObject] = position;
                    _posArray.Add(position);
                }
            }

            //int minHeight = AndroidUtilities.dp(120);
            //int minWidth = (int)(AndroidUtilities.dp(120) / (Math.Min(AndroidUtilities.displaySize.x, AndroidUtilities.displaySize.y) / (float)_maxSizeWidth));
            //int paddingsWidth = (int)(AndroidUtilities.dp(40) / (Math.Min(AndroidUtilities.displaySize.x, AndroidUtilities.displaySize.y) / (float)_maxSizeWidth));


            float maxAspectRatio = _maxSizeWidth / maxSizeHeight;
            averageAspectRatio = averageAspectRatio / count;

            if (!forceCalc && (count == 2 || count == 3 || count == 4))
            {
                if (count == 2)
                {
                    GroupedMessagePosition position1 = _posArray[0];
                    GroupedMessagePosition position2 = _posArray[1];
                    String pString = proportions.ToString();
                    if (pString.Equals("ww") && averageAspectRatio > 1.4 * maxAspectRatio && position1.AspectRatio - position2.AspectRatio < 0.2)
                    {
                        float height = (float)Math.Round(Math.Min(_maxSizeWidth / position1.AspectRatio, Math.Min(_maxSizeWidth / position2.AspectRatio, maxSizeHeight / 2.0f))) / maxSizeHeight;
                        position1.Set(0, 0, 0, 0, _maxSizeWidth, height, POSITION_FLAG_LEFT | POSITION_FLAG_RIGHT | POSITION_FLAG_TOP);
                        position2.Set(0, 0, 1, 1, _maxSizeWidth, height, POSITION_FLAG_LEFT | POSITION_FLAG_RIGHT | POSITION_FLAG_BOTTOM);

                        totalWidth = _maxSizeWidth;
                        totalHeight = height * 2;
                    }
                    else if (pString.Equals("ww") || pString.Equals("qq"))
                    {
                        int width = _maxSizeWidth / 2;
                        float height = (float)Math.Round(Math.Min(width / position1.AspectRatio, Math.Min(width / position2.AspectRatio, maxSizeHeight))) / maxSizeHeight;
                        position1.Set(0, 0, 0, 0, width, height, POSITION_FLAG_LEFT | POSITION_FLAG_BOTTOM | POSITION_FLAG_TOP);
                        position2.Set(1, 1, 0, 0, width, height, POSITION_FLAG_RIGHT | POSITION_FLAG_BOTTOM | POSITION_FLAG_TOP);
                        maxX = 1;

                        totalWidth = width + width;
                        totalHeight = height;
                    }
                    else
                    {
                        int secondWidth = (int)Math.Max(0.4f * _maxSizeWidth, Math.Round((_maxSizeWidth / position1.AspectRatio / (1.0f / position1.AspectRatio + 1.0f / position2.AspectRatio))));
                        int firstWidth = _maxSizeWidth - secondWidth;
                        if (firstWidth < minWidth)
                        {
                            int diff = minWidth - firstWidth;
                            firstWidth = minWidth;
                            secondWidth -= diff;
                        }

                        float height = (float)Math.Min(maxSizeHeight, Math.Round(Math.Min(firstWidth / position1.AspectRatio, secondWidth / position2.AspectRatio))) / maxSizeHeight;
                        position1.Set(0, 0, 0, 0, firstWidth, height, POSITION_FLAG_LEFT | POSITION_FLAG_BOTTOM | POSITION_FLAG_TOP);
                        position2.Set(1, 1, 0, 0, secondWidth, height, POSITION_FLAG_RIGHT | POSITION_FLAG_BOTTOM | POSITION_FLAG_TOP);
                        maxX = 1;

                        totalWidth = firstWidth + secondWidth;
                        totalHeight = height;
                    }
                }
                else if (count == 3)
                {
                    GroupedMessagePosition position1 = _posArray[0];
                    GroupedMessagePosition position2 = _posArray[1];
                    GroupedMessagePosition position3 = _posArray[2];
                    if (proportions[0] == 'n')
                    {
                        float thirdHeight = (float)Math.Min(maxSizeHeight * 0.5f, Math.Round(position2.AspectRatio * _maxSizeWidth / (position3.AspectRatio + position2.AspectRatio)));
                        float secondHeight = maxSizeHeight - thirdHeight;
                        int rightWidth = (int)Math.Max(minWidth, Math.Min(_maxSizeWidth * 0.5f, Math.Round(Math.Min(thirdHeight * position3.AspectRatio, secondHeight * position2.AspectRatio))));

                        int leftWidth = (int)Math.Round(Math.Min(maxSizeHeight * position1.AspectRatio + paddingsWidth, _maxSizeWidth - rightWidth));
                        position1.Set(0, 0, 0, 1, leftWidth, 1.0f, POSITION_FLAG_LEFT | POSITION_FLAG_BOTTOM | POSITION_FLAG_TOP);

                        position2.Set(1, 1, 0, 0, rightWidth, secondHeight / maxSizeHeight, POSITION_FLAG_RIGHT | POSITION_FLAG_TOP);

                        position3.Set(0, 1, 1, 1, rightWidth, thirdHeight / maxSizeHeight, POSITION_FLAG_RIGHT | POSITION_FLAG_BOTTOM);
                        position3.SpanSize = _maxSizeWidth;

                        position1.SiblingHeights = new float[] { thirdHeight / maxSizeHeight, secondHeight / maxSizeHeight };

                        if (isOut)
                        {
                            position1.SpanSize = _maxSizeWidth - rightWidth;
                        }
                        else
                        {
                            position2.SpanSize = _maxSizeWidth - leftWidth;
                            position3.LeftSpanOffset = leftWidth;
                        }
                        HasSibling = true;
                        maxX = 1;

                        totalWidth = leftWidth + rightWidth;
                        totalHeight = 1.0f;
                    }
                    else
                    {
                        float firstHeight = (float)Math.Round(Math.Min(_maxSizeWidth / position1.AspectRatio, (maxSizeHeight) * 0.66f)) / maxSizeHeight;
                        position1.Set(0, 1, 0, 0, _maxSizeWidth, firstHeight, POSITION_FLAG_LEFT | POSITION_FLAG_RIGHT | POSITION_FLAG_TOP);

                        int width = _maxSizeWidth / 2;
                        float secondHeight = (float)Math.Min(maxSizeHeight - firstHeight, Math.Round(Math.Min(width / position2.AspectRatio, width / position3.AspectRatio))) / maxSizeHeight;
                        position2.Set(0, 0, 1, 1, width, secondHeight, POSITION_FLAG_LEFT | POSITION_FLAG_BOTTOM);
                        position3.Set(1, 1, 1, 1, width, secondHeight, POSITION_FLAG_RIGHT | POSITION_FLAG_BOTTOM);
                        maxX = 1;

                        totalWidth = _maxSizeWidth;
                        totalHeight = firstHeight + secondHeight;
                    }
                }
                else if (count == 4)
                {
                    GroupedMessagePosition position1 = _posArray[0];
                    GroupedMessagePosition position2 = _posArray[1];
                    GroupedMessagePosition position3 = _posArray[2];
                    GroupedMessagePosition position4 = _posArray[3];
                    if (proportions[0] == 'w')
                    {
                        float h0 = (float)Math.Round(Math.Min(_maxSizeWidth / position1.AspectRatio, maxSizeHeight * 0.66f)) / maxSizeHeight;
                        position1.Set(0, 2, 0, 0, _maxSizeWidth, h0, POSITION_FLAG_LEFT | POSITION_FLAG_RIGHT | POSITION_FLAG_TOP);

                        float h = (float)Math.Round(_maxSizeWidth / (position2.AspectRatio + position3.AspectRatio + position4.AspectRatio));
                        int w0 = (int)Math.Max(minWidth, Math.Min(_maxSizeWidth * 0.4f, h * position2.AspectRatio));
                        int w2 = (int)Math.Max(Math.Max(minWidth, _maxSizeWidth * 0.33f), h * position4.AspectRatio);
                        int w1 = _maxSizeWidth - w0 - w2;
                        h = Math.Min(maxSizeHeight - h0, h);
                        h /= maxSizeHeight;
                        position2.Set(0, 0, 1, 1, w0, h, POSITION_FLAG_LEFT | POSITION_FLAG_BOTTOM);
                        position3.Set(1, 1, 1, 1, w1, h, POSITION_FLAG_BOTTOM);
                        position4.Set(2, 2, 1, 1, w2, h, POSITION_FLAG_RIGHT | POSITION_FLAG_BOTTOM);
                        maxX = 2;

                        totalWidth = _maxSizeWidth;
                        totalHeight = h0 + h;
                    }
                    else
                    {
                        int w = (int)Math.Max(minWidth, Math.Round(maxSizeHeight / (1.0f / position2.AspectRatio + 1.0f / position3.AspectRatio + 1.0f / _posArray[3].AspectRatio)));
                        float h0 = Math.Min(0.33f, Math.Max(minHeight, w / position2.AspectRatio) / maxSizeHeight);
                        float h1 = Math.Min(0.33f, Math.Max(minHeight, w / position3.AspectRatio) / maxSizeHeight);
                        float h2 = 1.0f - h0 - h1;
                        int w0 = (int)Math.Round(Math.Min(maxSizeHeight * position1.AspectRatio + paddingsWidth, _maxSizeWidth - w));

                        position1.Set(0, 0, 0, 2, w0, h0 + h1 + h2, POSITION_FLAG_LEFT | POSITION_FLAG_TOP | POSITION_FLAG_BOTTOM);

                        position2.Set(1, 1, 0, 0, w, h0, POSITION_FLAG_RIGHT | POSITION_FLAG_TOP);

                        position3.Set(0, 1, 1, 1, w, h1, POSITION_FLAG_RIGHT);
                        position3.SpanSize = _maxSizeWidth;

                        position4.Set(0, 1, 2, 2, w, h2, POSITION_FLAG_RIGHT | POSITION_FLAG_BOTTOM);
                        position4.SpanSize = _maxSizeWidth;

                        if (isOut)
                        {
                            position1.SpanSize = _maxSizeWidth - w;
                        }
                        else
                        {
                            position2.SpanSize = _maxSizeWidth - w0;
                            position3.LeftSpanOffset = w0;
                            position4.LeftSpanOffset = w0;
                        }
                        position1.SiblingHeights = new float[] { h0, h1, h2 };
                        HasSibling = true;
                        maxX = 1;

                        totalWidth = w + w0;
                        totalHeight = h0 + h1 + h2;
                    }
                }
            }
            else
            {
                float[] croppedRatios = new float[_posArray.Count];
                for (int a = 0; a < count; a++)
                {
                    if (averageAspectRatio > 1.1f)
                    {
                        croppedRatios[a] = Math.Max(1.0f, _posArray[a].AspectRatio);
                    }
                    else
                    {
                        croppedRatios[a] = Math.Min(1.0f, _posArray[a].AspectRatio);
                    }
                    croppedRatios[a] = Math.Max(0.66667f, Math.Min(1.7f, croppedRatios[a]));
                }

                int firstLine;
                int secondLine;
                int thirdLine;
                int fourthLine;
                List<MessageGroupedLayoutAttempt> attempts = new List<MessageGroupedLayoutAttempt>();
                for (firstLine = 1; firstLine < croppedRatios.Length; firstLine++)
                {
                    secondLine = croppedRatios.Length - firstLine;
                    if (firstLine > 3 || secondLine > 3)
                    {
                        continue;
                    }
                    attempts.Add(new MessageGroupedLayoutAttempt(firstLine, secondLine, MultiHeight(croppedRatios, 0, firstLine), MultiHeight(croppedRatios, firstLine, croppedRatios.Length)));
                }

                for (firstLine = 1; firstLine < croppedRatios.Length - 1; firstLine++)
                {
                    for (secondLine = 1; secondLine < croppedRatios.Length - firstLine; secondLine++)
                    {
                        thirdLine = croppedRatios.Length - firstLine - secondLine;
                        if (firstLine > 3 || secondLine > (averageAspectRatio < 0.85f ? 4 : 3) || thirdLine > 3)
                        {
                            continue;
                        }
                        attempts.Add(new MessageGroupedLayoutAttempt(firstLine, secondLine, thirdLine, MultiHeight(croppedRatios, 0, firstLine), MultiHeight(croppedRatios, firstLine, firstLine + secondLine), MultiHeight(croppedRatios, firstLine + secondLine, croppedRatios.Length)));
                    }
                }

                for (firstLine = 1; firstLine < croppedRatios.Length - 2; firstLine++)
                {
                    for (secondLine = 1; secondLine < croppedRatios.Length - firstLine; secondLine++)
                    {
                        for (thirdLine = 1; thirdLine < croppedRatios.Length - firstLine - secondLine; thirdLine++)
                        {
                            fourthLine = croppedRatios.Length - firstLine - secondLine - thirdLine;
                            if (firstLine > 3 || secondLine > 3 || thirdLine > 3 || fourthLine > 3)
                            {
                                continue;
                            }
                            attempts.Add(new MessageGroupedLayoutAttempt(firstLine, secondLine, thirdLine, fourthLine, MultiHeight(croppedRatios, 0, firstLine), MultiHeight(croppedRatios, firstLine, firstLine + secondLine), MultiHeight(croppedRatios, firstLine + secondLine, firstLine + secondLine + thirdLine), MultiHeight(croppedRatios, firstLine + secondLine + thirdLine, croppedRatios.Length)));
                        }
                    }
                }

                MessageGroupedLayoutAttempt optimal = null;
                float optimalDiff = 0.0f;
                float maxHeight = _maxSizeWidth / 3 * 4;
                for (int a = 0; a < attempts.Count; a++)
                {
                    MessageGroupedLayoutAttempt attempt = attempts[a];
                    float height = 0;
                    float minLineHeight = float.MaxValue;
                    for (int b = 0; b < attempt.Heights.Length; b++)
                    {
                        height += attempt.Heights[b];
                        if (attempt.Heights[b] < minLineHeight)
                        {
                            minLineHeight = attempt.Heights[b];
                        }
                    }

                    float diff = Math.Abs(height - maxHeight);
                    if (attempt.LineCounts.Length > 1)
                    {
                        if (attempt.LineCounts[0] > attempt.LineCounts[1] || (attempt.LineCounts.Length > 2 && attempt.LineCounts[1] > attempt.LineCounts[2]) || (attempt.LineCounts.Length > 3 && attempt.LineCounts[2] > attempt.LineCounts[3]))
                        {
                            diff *= 1.5f;
                        }
                    }

                    if (minLineHeight < minWidth)
                    {
                        diff *= 1.5f;
                    }

                    if (optimal == null || diff < optimalDiff)
                    {
                        optimal = attempt;
                        optimalDiff = diff;
                    }
                }
                if (optimal == null)
                {
                    return;
                }

                int index = 0;
                float y = 0.0f;

                for (int i = 0; i < optimal.LineCounts.Length; i++)
                {
                    int c = optimal.LineCounts[i];
                    float lineHeight = optimal.Heights[i];
                    int spanLeft = _maxSizeWidth;
                    GroupedMessagePosition posToFix = null;
                    maxX = Math.Max(maxX, c - 1);
                    for (int k = 0; k < c; k++)
                    {
                        float ratio = croppedRatios[index];
                        int width = (int)(ratio * lineHeight);
                        spanLeft -= width;
                        GroupedMessagePosition pos = _posArray[index];
                        int flags = 0;
                        if (i == 0)
                        {
                            flags |= POSITION_FLAG_TOP;
                        }
                        if (i == optimal.LineCounts.Length - 1)
                        {
                            flags |= POSITION_FLAG_BOTTOM;
                        }
                        if (k == 0)
                        {
                            flags |= POSITION_FLAG_LEFT;
                            if (isOut)
                            {
                                posToFix = pos;
                            }
                        }
                        if (k == c - 1)
                        {
                            flags |= POSITION_FLAG_RIGHT;
                            if (!isOut)
                            {
                                posToFix = pos;
                            }
                        }
                        pos.Set(k, k, i, i, width, lineHeight / maxSizeHeight, flags);
                        index++;
                    }
                    posToFix.Width += spanLeft;
                    posToFix.SpanSize += spanLeft;
                    y += lineHeight;
                }

                totalWidth = _maxSizeWidth;
                totalHeight = y / maxSizeHeight;
            }
            int avatarOffset = 108;
            for (int a = 0; a < count; a++)
            {
                //GroupedMessagePosition pos = _posArray[a];
                //if (isOut)
                //{
                //    if (pos.MinX == 0)
                //    {
                //        pos.SpanSize += firstSpanAdditionalSize;
                //    }
                //    if ((pos.Flags & POSITION_FLAG_RIGHT) != 0)
                //    {
                //        pos.IsEdge = true;
                //    }
                //}
                //else
                //{
                //    if (pos.MaxX == maxX || (pos.Flags & POSITION_FLAG_RIGHT) != 0)
                //    {
                //        pos.SpanSize += firstSpanAdditionalSize;
                //    }
                //    if ((pos.Flags & POSITION_FLAG_LEFT) != 0)
                //    {
                //        pos.IsEdge = true;
                //    }
                //}
                //TLMessage messageObject = Messages[a];
                //if (!isOut && messageObject.needDrawAvatar())
                //{
                //    if (pos.IsEdge)
                //    {
                //        if (pos.SpanSize != 1000)
                //        {
                //            pos.SpanSize += avatarOffset;
                //        }
                //        pos.Width += avatarOffset;
                //    }
                //    else if ((pos.Flags & POSITION_FLAG_RIGHT) != 0)
                //    {
                //        if (pos.SpanSize != 1000)
                //        {
                //            pos.SpanSize -= avatarOffset;
                //        }
                //        else if (pos.LeftSpanOffset != 0)
                //        {
                //            pos.LeftSpanOffset += avatarOffset;
                //        }
                //    }
                //}
            }

            Width = totalWidth;
            Height = totalHeight;
        }

        public static TLPhotoSizeBase GetClosestPhotoSizeWithSize(TLVector<TLPhotoSizeBase> sizes, int side)
        {
            return GetClosestPhotoSizeWithSize(sizes, side, false);
        }

        public static TLPhotoSizeBase GetClosestPhotoSizeWithSize(TLVector<TLPhotoSizeBase> sizes, int side, bool byMinSide)
        {
            if (sizes == null || sizes.Count == 0)
            {
                return null;
            }
            int lastSide = 0;
            TLPhotoSizeBase closestObject = null;
            for (int a = 0; a < sizes.Count; a++)
            {
                TLPhotoSizeBase obj = sizes[a];
                if (obj == null)
                {
                    continue;
                }

                int w = 0;
                int h = 0;
                var size = obj as TLPhotoSize;
                var cachedSize = obj as TLPhotoCachedSize;
                if (size != null)
                {
                    w = size.W.Value;
                    h = size.H.Value;
                }
                else if (cachedSize != null)
                {
                    w = cachedSize.W.Value;
                    h = cachedSize.H.Value;
                }

                if (byMinSide)
                {
                    int currentSide = h >= w ? w : h;
                    var closestSize = closestObject as TLPhotoSize;
                    if (closestObject == null || side > 100 && closestSize != null && closestSize.Location is TLFileLocation && ((TLFileLocation)closestSize.Location).DCId.Value == int.MinValue || obj is TLPhotoCachedSize || side > lastSide && lastSide < currentSide)
                    {
                        closestObject = obj;
                        lastSide = currentSide;
                    }
                }
                else
                {
                    int currentSide = w >= h ? w : h;
                    var closestSize = closestObject as TLPhotoSize;
                    if (closestObject == null || side > 100 && closestSize != null && closestSize.Location is TLFileLocation && ((TLFileLocation)closestSize.Location).DCId.Value == int.MinValue || obj is TLPhotoCachedSize || currentSide <= side && lastSide < currentSide)
                    {
                        closestObject = obj;
                        lastSide = currentSide;
                    }
                }
            }
            return closestObject;
        }
    }
}

// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;

namespace Telegram.EmojiPanel.Controls.Utilites
{
    public class Segment
    {
        public int LowerBound { get; private set; }
        public int UpperBound { get; private set; }

        public bool IsEmpty { get { return UpperBound < LowerBound; } }

        public Segment(int lowerBound, int upperBound)
        {
            LowerBound = lowerBound;
            UpperBound = upperBound;
        }

        public Segment()
            : this(0, -1)
        {
        }

        public override string ToString()
        {
            if (IsEmpty) return "[]";
            return String.Format("[{0},{1}]", LowerBound, UpperBound);
        }

        public void CompareToSegment(
            Segment otherSegment,
            out Segment thisMinusOther1,
            out Segment thisMinusOther2,
            out Segment intersection,
            out Segment otherMinusThis1,
            out Segment otherMinusThis2)
        {

            thisMinusOther1 = new Segment();
            thisMinusOther2 = new Segment();
            intersection = new Segment();
            otherMinusThis1 = new Segment();
            otherMinusThis2 = new Segment();


            if (this.IsEmpty)
            {
                otherMinusThis1 = otherSegment;
                return;
            }

            if (otherSegment.IsEmpty)
            {
                thisMinusOther1 = this;
                return;
            }

            if (this.UpperBound < otherSegment.LowerBound)
            {

                // do not intersect

                thisMinusOther1 = this;

                otherMinusThis1 = otherSegment;

                return;
            }


            if (this.LowerBound < otherSegment.LowerBound &&
                this.UpperBound >= otherSegment.LowerBound &&
                this.UpperBound <= otherSegment.UpperBound)
            {
                thisMinusOther1 = new Segment(this.LowerBound, otherSegment.LowerBound - 1);
                intersection = new Segment(otherSegment.LowerBound, this.UpperBound);
                otherMinusThis1 = new Segment(this.UpperBound + 1, otherSegment.UpperBound);
                return;
            }

            if (this.LowerBound >= otherSegment.LowerBound &&
                this.UpperBound <= otherSegment.UpperBound)
            {
                intersection = this;
                otherMinusThis1 = new Segment(otherSegment.LowerBound, this.LowerBound - 1);
                otherMinusThis2 = new Segment(this.UpperBound + 1, otherSegment.UpperBound);
                return;
            }

            otherSegment.CompareToSegment(this,
                out otherMinusThis1,
                out otherMinusThis2,
                out intersection,
                out thisMinusOther1,
                out thisMinusOther2);

        }
    }
}

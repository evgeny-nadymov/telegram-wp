using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;
using Telegram.EmojiPanel.Controls.Emoji;

namespace Telegram.EmojiPanel
{

    public static class Emoji
    {
        private static Dictionary<string, string> _dict;

        public static Dictionary<string, string> Dict
        {
            get
            {
                if (_dict == null)
                {
                    _dict = new Dictionary<string, string>();

                    InitializeDict();
                }

                return _dict;
            }
        }

        private static void InitializeDict()
        {
            _dict["002320E3"] = "002320E3";
            _dict["003020E3"] = "003020E3";
            _dict["003120E3"] = "003120E3";
            _dict["003220E3"] = "003220E3";
            _dict["003320E3"] = "003320E3";
            _dict["003420E3"] = "003420E3";
            _dict["003520E3"] = "003520E3";
            _dict["003620E3"] = "003620E3";
            _dict["003720E3"] = "003720E3";
            _dict["003820E3"] = "003820E3";
            _dict["003920E3"] = "003920E3";
            _dict["00A9"] = "00A9";
            _dict["00AE"] = "00AE";
            _dict["203C"] = "203C";
            _dict["2049"] = "2049";
            _dict["2122"] = "2122";
            _dict["2139"] = "2139";
            _dict["2194"] = "2194";
            _dict["2195"] = "2195";
            _dict["2196"] = "2196";
            _dict["2197"] = "2197";
            _dict["2198"] = "2198";
            _dict["2199"] = "2199";
            _dict["21A9"] = "21A9";
            _dict["21AA"] = "21AA";
            _dict["231A"] = "231A";
            _dict["231B"] = "231B";
            _dict["23E9"] = "23E9";
            _dict["23EA"] = "23EA";
            _dict["23EB"] = "23EB";
            _dict["23EC"] = "23EC";
            _dict["23F0"] = "23F0";
            _dict["23F3"] = "23F3";
            _dict["24C2"] = "24C2";
            _dict["25AA"] = "25AA";
            _dict["25AB"] = "25AB";
            _dict["25B6"] = "25B6";
            _dict["25C0"] = "25C0";
            _dict["25FB"] = "25FB";
            _dict["25FC"] = "25FC";
            _dict["25FD"] = "25FD";
            _dict["25FE"] = "25FE";
            _dict["2600"] = "2600";
            _dict["2601"] = "2601";
            _dict["260E"] = "260E";
            _dict["2611"] = "2611";
            _dict["2614"] = "2614";
            _dict["2615"] = "2615";
            _dict["261D"] = "261D";
            _dict["263A"] = "263A";
            _dict["2648"] = "2648";
            _dict["2649"] = "2649";
            _dict["264A"] = "264A";
            _dict["264B"] = "264B";
            _dict["264C"] = "264C";
            _dict["264D"] = "264D";
            _dict["264E"] = "264E";
            _dict["264F"] = "264F";
            _dict["2650"] = "2650";
            _dict["2651"] = "2651";
            _dict["2652"] = "2652";
            _dict["2653"] = "2653";
            _dict["2660"] = "2660";
            _dict["2663"] = "2663";
            _dict["2665"] = "2665";
            _dict["2666"] = "2666";
            _dict["2668"] = "2668";
            _dict["267B"] = "267B";
            _dict["267F"] = "267F";
            _dict["2693"] = "2693";
            _dict["26A0"] = "26A0";
            _dict["26A1"] = "26A1";
            _dict["26AA"] = "26AA";
            _dict["26AB"] = "26AB";
            _dict["26BD"] = "26BD";
            _dict["26BE"] = "26BE";
            _dict["26C4"] = "26C4";
            _dict["26C5"] = "26C5";
            _dict["26CE"] = "26CE";
            _dict["26D4"] = "26D4";
            _dict["26EA"] = "26EA";
            _dict["26F2"] = "26F2";
            _dict["26F3"] = "26F3";
            _dict["26F5"] = "26F5";
            _dict["26FA"] = "26FA";
            _dict["26FD"] = "26FD";
            _dict["2702"] = "2702";
            _dict["2705"] = "2705";
            _dict["2708"] = "2708";
            _dict["2709"] = "2709";
            _dict["270A"] = "270A";
            _dict["270B"] = "270B";
            _dict["270C"] = "270C";
            _dict["270F"] = "270F";
            _dict["2712"] = "2712";
            _dict["2714"] = "2714";
            _dict["2716"] = "2716";
            _dict["2728"] = "2728";
            _dict["2733"] = "2733";
            _dict["2734"] = "2734";
            _dict["2744"] = "2744";
            _dict["2747"] = "2747";
            _dict["274C"] = "274C";
            _dict["274E"] = "274E";
            _dict["2753"] = "2753";
            _dict["2754"] = "2754";
            _dict["2755"] = "2755";
            _dict["2757"] = "2757";
            _dict["2764"] = "2764";
            _dict["2795"] = "2795";
            _dict["2796"] = "2796";
            _dict["2797"] = "2797";
            _dict["27A1"] = "27A1";
            _dict["27B0"] = "27B0";
            _dict["27BF"] = "27BF";
            _dict["2934"] = "2934";
            _dict["2935"] = "2935";
            _dict["2B05"] = "2B05";
            _dict["2B06"] = "2B06";
            _dict["2B07"] = "2B07";
            _dict["2B1B"] = "2B1B";
            _dict["2B1C"] = "2B1C";
            _dict["2B50"] = "2B50";
            _dict["2B55"] = "2B55";
            _dict["3030"] = "3030";
            _dict["303D"] = "303D";
            _dict["3297"] = "3297";
            _dict["3299"] = "3299";
            _dict["D83CDC04"] = "D83CDC04";
            _dict["D83CDCCF"] = "D83CDCCF";
            _dict["D83CDD70"] = "D83CDD70";
            _dict["D83CDD71"] = "D83CDD71";
            _dict["D83CDD7E"] = "D83CDD7E";
            _dict["D83CDD7F"] = "D83CDD7F";
            _dict["D83CDD8E"] = "D83CDD8E";
            _dict["D83CDD91"] = "D83CDD91";
            _dict["D83CDD92"] = "D83CDD92";
            _dict["D83CDD93"] = "D83CDD93";
            _dict["D83CDD94"] = "D83CDD94";
            _dict["D83CDD95"] = "D83CDD95";
            _dict["D83CDD96"] = "D83CDD96";
            _dict["D83CDD97"] = "D83CDD97";
            _dict["D83CDD98"] = "D83CDD98";
            _dict["D83CDD99"] = "D83CDD99";
            _dict["D83CDD9A"] = "D83CDD9A";
            _dict["D83CDDE8D83CDDF3"] = "D83CDDE8D83CDDF3";
            _dict["D83CDDE9D83CDDEA"] = "D83CDDE9D83CDDEA";
            _dict["D83CDDEAD83CDDF8"] = "D83CDDEAD83CDDF8";
            _dict["D83CDDEBD83CDDF7"] = "D83CDDEBD83CDDF7";
            _dict["D83CDDECD83CDDE7"] = "D83CDDECD83CDDE7";
            _dict["D83CDDEED83CDDF9"] = "D83CDDEED83CDDF9";
            _dict["D83CDDEFD83CDDF5"] = "D83CDDEFD83CDDF5";
            _dict["D83CDDF0D83CDDF7"] = "D83CDDF0D83CDDF7";
            _dict["D83CDDF7D83CDDFA"] = "D83CDDF7D83CDDFA";
            _dict["D83CDDFAD83CDDF8"] = "D83CDDFAD83CDDF8";
            _dict["D83CDE01"] = "D83CDE01";
            _dict["D83CDE02"] = "D83CDE02";
            _dict["D83CDE1A"] = "D83CDE1A";
            _dict["D83CDE2F"] = "D83CDE2F";
            _dict["D83CDE32"] = "D83CDE32";
            _dict["D83CDE33"] = "D83CDE33";
            _dict["D83CDE34"] = "D83CDE34";
            _dict["D83CDE35"] = "D83CDE35";
            _dict["D83CDE36"] = "D83CDE36";
            _dict["D83CDE37"] = "D83CDE37";
            _dict["D83CDE38"] = "D83CDE38";
            _dict["D83CDE39"] = "D83CDE39";
            _dict["D83CDE3A"] = "D83CDE3A";
            _dict["D83CDE50"] = "D83CDE50";
            _dict["D83CDE51"] = "D83CDE51";
            _dict["D83CDF00"] = "D83CDF00";
            _dict["D83CDF01"] = "D83CDF01";
            _dict["D83CDF02"] = "D83CDF02";
            _dict["D83CDF03"] = "D83CDF03";
            _dict["D83CDF04"] = "D83CDF04";
            _dict["D83CDF05"] = "D83CDF05";
            _dict["D83CDF06"] = "D83CDF06";
            _dict["D83CDF07"] = "D83CDF07";
            _dict["D83CDF08"] = "D83CDF08";
            _dict["D83CDF09"] = "D83CDF09";
            _dict["D83CDF0A"] = "D83CDF0A";
            _dict["D83CDF0B"] = "D83CDF0B";
            _dict["D83CDF0C"] = "D83CDF0C";
            _dict["D83CDF0D"] = "D83CDF0D";
            _dict["D83CDF0E"] = "D83CDF0E";
            _dict["D83CDF0F"] = "D83CDF0F";
            _dict["D83CDF10"] = "D83CDF10";
            _dict["D83CDF11"] = "D83CDF11";
            _dict["D83CDF12"] = "D83CDF12";
            _dict["D83CDF13"] = "D83CDF13";
            _dict["D83CDF14"] = "D83CDF14";
            _dict["D83CDF15"] = "D83CDF15";
            _dict["D83CDF16"] = "D83CDF16";
            _dict["D83CDF17"] = "D83CDF17";
            _dict["D83CDF18"] = "D83CDF18";
            _dict["D83CDF19"] = "D83CDF19";
            _dict["D83CDF1A"] = "D83CDF1A";
            _dict["D83CDF1B"] = "D83CDF1B";
            _dict["D83CDF1C"] = "D83CDF1C";
            _dict["D83CDF1D"] = "D83CDF1D";
            _dict["D83CDF1E"] = "D83CDF1E";
            _dict["D83CDF1F"] = "D83CDF1F";
            _dict["D83CDF20"] = "D83CDF20";
            _dict["D83CDF30"] = "D83CDF30";
            _dict["D83CDF31"] = "D83CDF31";
            _dict["D83CDF32"] = "D83CDF32";
            _dict["D83CDF33"] = "D83CDF33";
            _dict["D83CDF34"] = "D83CDF34";
            _dict["D83CDF35"] = "D83CDF35";
            _dict["D83CDF37"] = "D83CDF37";
            _dict["D83CDF38"] = "D83CDF38";
            _dict["D83CDF39"] = "D83CDF39";
            _dict["D83CDF3A"] = "D83CDF3A";
            _dict["D83CDF3B"] = "D83CDF3B";
            _dict["D83CDF3C"] = "D83CDF3C";
            _dict["D83CDF3D"] = "D83CDF3D";
            _dict["D83CDF3E"] = "D83CDF3E";
            _dict["D83CDF3F"] = "D83CDF3F";
            _dict["D83CDF40"] = "D83CDF40";
            _dict["D83CDF41"] = "D83CDF41";
            _dict["D83CDF42"] = "D83CDF42";
            _dict["D83CDF43"] = "D83CDF43";
            _dict["D83CDF44"] = "D83CDF44";
            _dict["D83CDF45"] = "D83CDF45";
            _dict["D83CDF46"] = "D83CDF46";
            _dict["D83CDF47"] = "D83CDF47";
            _dict["D83CDF48"] = "D83CDF48";
            _dict["D83CDF49"] = "D83CDF49";
            _dict["D83CDF4A"] = "D83CDF4A";
            _dict["D83CDF4B"] = "D83CDF4B";
            _dict["D83CDF4C"] = "D83CDF4C";
            _dict["D83CDF4D"] = "D83CDF4D";
            _dict["D83CDF4E"] = "D83CDF4E";
            _dict["D83CDF4F"] = "D83CDF4F";
            _dict["D83CDF50"] = "D83CDF50";
            _dict["D83CDF51"] = "D83CDF51";
            _dict["D83CDF52"] = "D83CDF52";
            _dict["D83CDF53"] = "D83CDF53";
            _dict["D83CDF54"] = "D83CDF54";
            _dict["D83CDF55"] = "D83CDF55";
            _dict["D83CDF56"] = "D83CDF56";
            _dict["D83CDF57"] = "D83CDF57";
            _dict["D83CDF58"] = "D83CDF58";
            _dict["D83CDF59"] = "D83CDF59";
            _dict["D83CDF5A"] = "D83CDF5A";
            _dict["D83CDF5B"] = "D83CDF5B";
            _dict["D83CDF5C"] = "D83CDF5C";
            _dict["D83CDF5D"] = "D83CDF5D";
            _dict["D83CDF5E"] = "D83CDF5E";
            _dict["D83CDF5F"] = "D83CDF5F";
            _dict["D83CDF60"] = "D83CDF60";
            _dict["D83CDF61"] = "D83CDF61";
            _dict["D83CDF62"] = "D83CDF62";
            _dict["D83CDF63"] = "D83CDF63";
            _dict["D83CDF64"] = "D83CDF64";
            _dict["D83CDF65"] = "D83CDF65";
            _dict["D83CDF66"] = "D83CDF66";
            _dict["D83CDF67"] = "D83CDF67";
            _dict["D83CDF68"] = "D83CDF68";
            _dict["D83CDF69"] = "D83CDF69";
            _dict["D83CDF6A"] = "D83CDF6A";
            _dict["D83CDF6B"] = "D83CDF6B";
            _dict["D83CDF6C"] = "D83CDF6C";
            _dict["D83CDF6D"] = "D83CDF6D";
            _dict["D83CDF6E"] = "D83CDF6E";
            _dict["D83CDF6F"] = "D83CDF6F";
            _dict["D83CDF70"] = "D83CDF70";
            _dict["D83CDF71"] = "D83CDF71";
            _dict["D83CDF72"] = "D83CDF72";
            _dict["D83CDF73"] = "D83CDF73";
            _dict["D83CDF74"] = "D83CDF74";
            _dict["D83CDF75"] = "D83CDF75";
            _dict["D83CDF76"] = "D83CDF76";
            _dict["D83CDF77"] = "D83CDF77";
            _dict["D83CDF78"] = "D83CDF78";
            _dict["D83CDF79"] = "D83CDF79";
            _dict["D83CDF7A"] = "D83CDF7A";
            _dict["D83CDF7B"] = "D83CDF7B";
            _dict["D83CDF7C"] = "D83CDF7C";
            _dict["D83CDF80"] = "D83CDF80";
            _dict["D83CDF81"] = "D83CDF81";
            _dict["D83CDF82"] = "D83CDF82";
            _dict["D83CDF83"] = "D83CDF83";
            _dict["D83CDF84"] = "D83CDF84";
            _dict["D83CDF85"] = "D83CDF85";
            _dict["D83CDF86"] = "D83CDF86";
            _dict["D83CDF87"] = "D83CDF87";
            _dict["D83CDF88"] = "D83CDF88";
            _dict["D83CDF89"] = "D83CDF89";
            _dict["D83CDF8A"] = "D83CDF8A";
            _dict["D83CDF8B"] = "D83CDF8B";
            _dict["D83CDF8C"] = "D83CDF8C";
            _dict["D83CDF8D"] = "D83CDF8D";
            _dict["D83CDF8E"] = "D83CDF8E";
            _dict["D83CDF8F"] = "D83CDF8F";
            _dict["D83CDF90"] = "D83CDF90";
            _dict["D83CDF91"] = "D83CDF91";
            _dict["D83CDF92"] = "D83CDF92";
            _dict["D83CDF93"] = "D83CDF93";
            _dict["D83CDFA0"] = "D83CDFA0";
            _dict["D83CDFA1"] = "D83CDFA1";
            _dict["D83CDFA2"] = "D83CDFA2";
            _dict["D83CDFA3"] = "D83CDFA3";
            _dict["D83CDFA4"] = "D83CDFA4";
            _dict["D83CDFA5"] = "D83CDFA5";
            _dict["D83CDFA6"] = "D83CDFA6";
            _dict["D83CDFA7"] = "D83CDFA7";
            _dict["D83CDFA8"] = "D83CDFA8";
            _dict["D83CDFA9"] = "D83CDFA9";
            _dict["D83CDFAA"] = "D83CDFAA";
            _dict["D83CDFAB"] = "D83CDFAB";
            _dict["D83CDFAC"] = "D83CDFAC";
            _dict["D83CDFAD"] = "D83CDFAD";
            _dict["D83CDFAE"] = "D83CDFAE";
            _dict["D83CDFAF"] = "D83CDFAF";
            _dict["D83CDFB0"] = "D83CDFB0";
            _dict["D83CDFB1"] = "D83CDFB1";
            _dict["D83CDFB2"] = "D83CDFB2";
            _dict["D83CDFB3"] = "D83CDFB3";
            _dict["D83CDFB4"] = "D83CDFB4";
            _dict["D83CDFB5"] = "D83CDFB5";
            _dict["D83CDFB6"] = "D83CDFB6";
            _dict["D83CDFB7"] = "D83CDFB7";
            _dict["D83CDFB8"] = "D83CDFB8";
            _dict["D83CDFB9"] = "D83CDFB9";
            _dict["D83CDFBA"] = "D83CDFBA";
            _dict["D83CDFBB"] = "D83CDFBB";
            _dict["D83CDFBC"] = "D83CDFBC";
            _dict["D83CDFBD"] = "D83CDFBD";
            _dict["D83CDFBE"] = "D83CDFBE";
            _dict["D83CDFBF"] = "D83CDFBF";
            _dict["D83CDFC0"] = "D83CDFC0";
            _dict["D83CDFC1"] = "D83CDFC1";
            _dict["D83CDFC2"] = "D83CDFC2";
            _dict["D83CDFC3"] = "D83CDFC3";
            _dict["D83CDFC4"] = "D83CDFC4";
            _dict["D83CDFC6"] = "D83CDFC6";
            _dict["D83CDFC7"] = "D83CDFC7";
            _dict["D83CDFC8"] = "D83CDFC8";
            _dict["D83CDFC9"] = "D83CDFC9";
            _dict["D83CDFCA"] = "D83CDFCA";
            _dict["D83CDFE0"] = "D83CDFE0";
            _dict["D83CDFE1"] = "D83CDFE1";
            _dict["D83CDFE2"] = "D83CDFE2";
            _dict["D83CDFE3"] = "D83CDFE3";
            _dict["D83CDFE4"] = "D83CDFE4";
            _dict["D83CDFE5"] = "D83CDFE5";
            _dict["D83CDFE6"] = "D83CDFE6";
            _dict["D83CDFE7"] = "D83CDFE7";
            _dict["D83CDFE8"] = "D83CDFE8";
            _dict["D83CDFE9"] = "D83CDFE9";
            _dict["D83CDFEA"] = "D83CDFEA";
            _dict["D83CDFEB"] = "D83CDFEB";
            _dict["D83CDFEC"] = "D83CDFEC";
            _dict["D83CDFED"] = "D83CDFED";
            _dict["D83CDFEE"] = "D83CDFEE";
            _dict["D83CDFEF"] = "D83CDFEF";
            _dict["D83CDFF0"] = "D83CDFF0";
            _dict["D83DDC00"] = "D83DDC00";
            _dict["D83DDC01"] = "D83DDC01";
            _dict["D83DDC02"] = "D83DDC02";
            _dict["D83DDC03"] = "D83DDC03";
            _dict["D83DDC04"] = "D83DDC04";
            _dict["D83DDC05"] = "D83DDC05";
            _dict["D83DDC06"] = "D83DDC06";
            _dict["D83DDC07"] = "D83DDC07";
            _dict["D83DDC08"] = "D83DDC08";
            _dict["D83DDC09"] = "D83DDC09";
            _dict["D83DDC0A"] = "D83DDC0A";
            _dict["D83DDC0B"] = "D83DDC0B";
            _dict["D83DDC0C"] = "D83DDC0C";
            _dict["D83DDC0D"] = "D83DDC0D";
            _dict["D83DDC0E"] = "D83DDC0E";
            _dict["D83DDC0F"] = "D83DDC0F";
            _dict["D83DDC10"] = "D83DDC10";
            _dict["D83DDC11"] = "D83DDC11";
            _dict["D83DDC12"] = "D83DDC12";
            _dict["D83DDC13"] = "D83DDC13";
            _dict["D83DDC14"] = "D83DDC14";
            _dict["D83DDC15"] = "D83DDC15";
            _dict["D83DDC16"] = "D83DDC16";
            _dict["D83DDC17"] = "D83DDC17";
            _dict["D83DDC18"] = "D83DDC18";
            _dict["D83DDC19"] = "D83DDC19";
            _dict["D83DDC1A"] = "D83DDC1A";
            _dict["D83DDC1B"] = "D83DDC1B";
            _dict["D83DDC1C"] = "D83DDC1C";
            _dict["D83DDC1D"] = "D83DDC1D";
            _dict["D83DDC1E"] = "D83DDC1E";
            _dict["D83DDC1F"] = "D83DDC1F";
            _dict["D83DDC20"] = "D83DDC20";
            _dict["D83DDC21"] = "D83DDC21";
            _dict["D83DDC22"] = "D83DDC22";
            _dict["D83DDC23"] = "D83DDC23";
            _dict["D83DDC24"] = "D83DDC24";
            _dict["D83DDC25"] = "D83DDC25";
            _dict["D83DDC26"] = "D83DDC26";
            _dict["D83DDC27"] = "D83DDC27";
            _dict["D83DDC28"] = "D83DDC28";
            _dict["D83DDC29"] = "D83DDC29";
            _dict["D83DDC2A"] = "D83DDC2A";
            _dict["D83DDC2B"] = "D83DDC2B";
            _dict["D83DDC2C"] = "D83DDC2C";
            _dict["D83DDC2D"] = "D83DDC2D";
            _dict["D83DDC2E"] = "D83DDC2E";
            _dict["D83DDC2F"] = "D83DDC2F";
            _dict["D83DDC30"] = "D83DDC30";
            _dict["D83DDC31"] = "D83DDC31";
            _dict["D83DDC32"] = "D83DDC32";
            _dict["D83DDC33"] = "D83DDC33";
            _dict["D83DDC34"] = "D83DDC34";
            _dict["D83DDC35"] = "D83DDC35";
            _dict["D83DDC36"] = "D83DDC36";
            _dict["D83DDC37"] = "D83DDC37";
            _dict["D83DDC38"] = "D83DDC38";
            _dict["D83DDC39"] = "D83DDC39";
            _dict["D83DDC3A"] = "D83DDC3A";
            _dict["D83DDC3B"] = "D83DDC3B";
            _dict["D83DDC3C"] = "D83DDC3C";
            _dict["D83DDC3D"] = "D83DDC3D";
            _dict["D83DDC3E"] = "D83DDC3E";
            _dict["D83DDC40"] = "D83DDC40";
            _dict["D83DDC42"] = "D83DDC42";
            _dict["D83DDC43"] = "D83DDC43";
            _dict["D83DDC44"] = "D83DDC44";
            _dict["D83DDC45"] = "D83DDC45";
            _dict["D83DDC46"] = "D83DDC46";
            _dict["D83DDC47"] = "D83DDC47";
            _dict["D83DDC48"] = "D83DDC48";
            _dict["D83DDC49"] = "D83DDC49";
            _dict["D83DDC4A"] = "D83DDC4A";
            _dict["D83DDC4B"] = "D83DDC4B";
            _dict["D83DDC4C"] = "D83DDC4C";
            _dict["D83DDC4D"] = "D83DDC4D";
            _dict["D83DDC4E"] = "D83DDC4E";
            _dict["D83DDC4F"] = "D83DDC4F";
            _dict["D83DDC50"] = "D83DDC50";
            _dict["D83DDC51"] = "D83DDC51";
            _dict["D83DDC52"] = "D83DDC52";
            _dict["D83DDC53"] = "D83DDC53";
            _dict["D83DDC54"] = "D83DDC54";
            _dict["D83DDC55"] = "D83DDC55";
            _dict["D83DDC56"] = "D83DDC56";
            _dict["D83DDC57"] = "D83DDC57";
            _dict["D83DDC58"] = "D83DDC58";
            _dict["D83DDC59"] = "D83DDC59";
            _dict["D83DDC5A"] = "D83DDC5A";
            _dict["D83DDC5B"] = "D83DDC5B";
            _dict["D83DDC5C"] = "D83DDC5C";
            _dict["D83DDC5D"] = "D83DDC5D";
            _dict["D83DDC5E"] = "D83DDC5E";
            _dict["D83DDC5F"] = "D83DDC5F";
            _dict["D83DDC60"] = "D83DDC60";
            _dict["D83DDC61"] = "D83DDC61";
            _dict["D83DDC62"] = "D83DDC62";
            _dict["D83DDC63"] = "D83DDC63";
            _dict["D83DDC64"] = "D83DDC64";
            _dict["D83DDC65"] = "D83DDC65";
            _dict["D83DDC66"] = "D83DDC66";
            _dict["D83DDC67"] = "D83DDC67";
            _dict["D83DDC68"] = "D83DDC68";
            _dict["D83DDC69"] = "D83DDC69";
            _dict["D83DDC6A"] = "D83DDC6A";
            _dict["D83DDC6B"] = "D83DDC6B";
            _dict["D83DDC6C"] = "D83DDC6C";
            _dict["D83DDC6D"] = "D83DDC6D";
            _dict["D83DDC6E"] = "D83DDC6E";
            _dict["D83DDC6F"] = "D83DDC6F";
            _dict["D83DDC70"] = "D83DDC70";
            _dict["D83DDC71"] = "D83DDC71";
            _dict["D83DDC72"] = "D83DDC72";
            _dict["D83DDC73"] = "D83DDC73";
            _dict["D83DDC74"] = "D83DDC74";
            _dict["D83DDC75"] = "D83DDC75";
            _dict["D83DDC76"] = "D83DDC76";
            _dict["D83DDC77"] = "D83DDC77";
            _dict["D83DDC78"] = "D83DDC78";
            _dict["D83DDC79"] = "D83DDC79";
            _dict["D83DDC7A"] = "D83DDC7A";
            _dict["D83DDC7B"] = "D83DDC7B";
            _dict["D83DDC7C"] = "D83DDC7C";
            _dict["D83DDC7D"] = "D83DDC7D";
            _dict["D83DDC7E"] = "D83DDC7E";
            _dict["D83DDC7F"] = "D83DDC7F";
            _dict["D83DDC80"] = "D83DDC80";
            _dict["D83DDC81"] = "D83DDC81";
            _dict["D83DDC82"] = "D83DDC82";
            _dict["D83DDC83"] = "D83DDC83";
            _dict["D83DDC84"] = "D83DDC84";
            _dict["D83DDC85"] = "D83DDC85";
            _dict["D83DDC86"] = "D83DDC86";
            _dict["D83DDC87"] = "D83DDC87";
            _dict["D83DDC88"] = "D83DDC88";
            _dict["D83DDC89"] = "D83DDC89";
            _dict["D83DDC8A"] = "D83DDC8A";
            _dict["D83DDC8B"] = "D83DDC8B";
            _dict["D83DDC8C"] = "D83DDC8C";
            _dict["D83DDC8D"] = "D83DDC8D";
            _dict["D83DDC8E"] = "D83DDC8E";
            _dict["D83DDC8F"] = "D83DDC8F";
            _dict["D83DDC90"] = "D83DDC90";
            _dict["D83DDC91"] = "D83DDC91";
            _dict["D83DDC92"] = "D83DDC92";
            _dict["D83DDC93"] = "D83DDC93";
            _dict["D83DDC94"] = "D83DDC94";
            _dict["D83DDC95"] = "D83DDC95";
            _dict["D83DDC96"] = "D83DDC96";
            _dict["D83DDC97"] = "D83DDC97";
            _dict["D83DDC98"] = "D83DDC98";
            _dict["D83DDC99"] = "D83DDC99";
            _dict["D83DDC9A"] = "D83DDC9A";
            _dict["D83DDC9B"] = "D83DDC9B";
            _dict["D83DDC9C"] = "D83DDC9C";
            _dict["D83DDC9D"] = "D83DDC9D";
            _dict["D83DDC9E"] = "D83DDC9E";
            _dict["D83DDC9F"] = "D83DDC9F";
            _dict["D83DDCA0"] = "D83DDCA0";
            _dict["D83DDCA1"] = "D83DDCA1";
            _dict["D83DDCA2"] = "D83DDCA2";
            _dict["D83DDCA3"] = "D83DDCA3";
            _dict["D83DDCA4"] = "D83DDCA4";
            _dict["D83DDCA5"] = "D83DDCA5";
            _dict["D83DDCA6"] = "D83DDCA6";
            _dict["D83DDCA7"] = "D83DDCA7";
            _dict["D83DDCA8"] = "D83DDCA8";
            _dict["D83DDCA9"] = "D83DDCA9";
            _dict["D83DDCAA"] = "D83DDCAA";
            _dict["D83DDCAB"] = "D83DDCAB";
            _dict["D83DDCAC"] = "D83DDCAC";
            _dict["D83DDCAD"] = "D83DDCAD";
            _dict["D83DDCAE"] = "D83DDCAE";
            _dict["D83DDCAF"] = "D83DDCAF";
            _dict["D83DDCB0"] = "D83DDCB0";
            _dict["D83DDCB1"] = "D83DDCB1";
            _dict["D83DDCB2"] = "D83DDCB2";
            _dict["D83DDCB3"] = "D83DDCB3";
            _dict["D83DDCB4"] = "D83DDCB4";
            _dict["D83DDCB5"] = "D83DDCB5";
            _dict["D83DDCB6"] = "D83DDCB6";
            _dict["D83DDCB7"] = "D83DDCB7";
            _dict["D83DDCB8"] = "D83DDCB8";
            _dict["D83DDCB9"] = "D83DDCB9";
            _dict["D83DDCBA"] = "D83DDCBA";
            _dict["D83DDCBB"] = "D83DDCBB";
            _dict["D83DDCBC"] = "D83DDCBC";
            _dict["D83DDCBD"] = "D83DDCBD";
            _dict["D83DDCBE"] = "D83DDCBE";
            _dict["D83DDCBF"] = "D83DDCBF";
            _dict["D83DDCC0"] = "D83DDCC0";
            _dict["D83DDCC1"] = "D83DDCC1";
            _dict["D83DDCC2"] = "D83DDCC2";
            _dict["D83DDCC3"] = "D83DDCC3";
            _dict["D83DDCC4"] = "D83DDCC4";
            _dict["D83DDCC5"] = "D83DDCC5";
            _dict["D83DDCC6"] = "D83DDCC6";
            _dict["D83DDCC7"] = "D83DDCC7";
            _dict["D83DDCC8"] = "D83DDCC8";
            _dict["D83DDCC9"] = "D83DDCC9";
            _dict["D83DDCCA"] = "D83DDCCA";
            _dict["D83DDCCB"] = "D83DDCCB";
            _dict["D83DDCCC"] = "D83DDCCC";
            _dict["D83DDCCD"] = "D83DDCCD";
            _dict["D83DDCCE"] = "D83DDCCE";
            _dict["D83DDCCF"] = "D83DDCCF";
            _dict["D83DDCD0"] = "D83DDCD0";
            _dict["D83DDCD1"] = "D83DDCD1";
            _dict["D83DDCD2"] = "D83DDCD2";
            _dict["D83DDCD3"] = "D83DDCD3";
            _dict["D83DDCD4"] = "D83DDCD4";
            _dict["D83DDCD5"] = "D83DDCD5";
            _dict["D83DDCD6"] = "D83DDCD6";
            _dict["D83DDCD7"] = "D83DDCD7";
            _dict["D83DDCD8"] = "D83DDCD8";
            _dict["D83DDCD9"] = "D83DDCD9";
            _dict["D83DDCDA"] = "D83DDCDA";
            _dict["D83DDCDB"] = "D83DDCDB";
            _dict["D83DDCDC"] = "D83DDCDC";
            _dict["D83DDCDD"] = "D83DDCDD";
            _dict["D83DDCDE"] = "D83DDCDE";
            _dict["D83DDCDF"] = "D83DDCDF";
            _dict["D83DDCE0"] = "D83DDCE0";
            _dict["D83DDCE1"] = "D83DDCE1";
            _dict["D83DDCE2"] = "D83DDCE2";
            _dict["D83DDCE3"] = "D83DDCE3";
            _dict["D83DDCE4"] = "D83DDCE4";
            _dict["D83DDCE5"] = "D83DDCE5";
            _dict["D83DDCE6"] = "D83DDCE6";
            _dict["D83DDCE7"] = "D83DDCE7";
            _dict["D83DDCE8"] = "D83DDCE8";
            _dict["D83DDCE9"] = "D83DDCE9";
            _dict["D83DDCEA"] = "D83DDCEA";
            _dict["D83DDCEB"] = "D83DDCEB";
            _dict["D83DDCEC"] = "D83DDCEC";
            _dict["D83DDCED"] = "D83DDCED";
            _dict["D83DDCEE"] = "D83DDCEE";
            _dict["D83DDCEF"] = "D83DDCEF";
            _dict["D83DDCF0"] = "D83DDCF0";
            _dict["D83DDCF1"] = "D83DDCF1";
            _dict["D83DDCF2"] = "D83DDCF2";
            _dict["D83DDCF3"] = "D83DDCF3";
            _dict["D83DDCF4"] = "D83DDCF4";
            _dict["D83DDCF5"] = "D83DDCF5";
            _dict["D83DDCF6"] = "D83DDCF6";
            _dict["D83DDCF7"] = "D83DDCF7";
            _dict["D83DDCF9"] = "D83DDCF9";
            _dict["D83DDCFA"] = "D83DDCFA";
            _dict["D83DDCFB"] = "D83DDCFB";
            _dict["D83DDCFC"] = "D83DDCFC";
            _dict["D83DDD00"] = "D83DDD00";
            _dict["D83DDD01"] = "D83DDD01";
            _dict["D83DDD02"] = "D83DDD02";
            _dict["D83DDD03"] = "D83DDD03";
            _dict["D83DDD04"] = "D83DDD04";
            _dict["D83DDD05"] = "D83DDD05";
            _dict["D83DDD06"] = "D83DDD06";
            _dict["D83DDD07"] = "D83DDD07";
            _dict["D83DDD08"] = "D83DDD08";
            _dict["D83DDD09"] = "D83DDD09";
            _dict["D83DDD0A"] = "D83DDD0A";
            _dict["D83DDD0B"] = "D83DDD0B";
            _dict["D83DDD0C"] = "D83DDD0C";
            _dict["D83DDD0D"] = "D83DDD0D";
            _dict["D83DDD0E"] = "D83DDD0E";
            _dict["D83DDD0F"] = "D83DDD0F";
            _dict["D83DDD10"] = "D83DDD10";
            _dict["D83DDD11"] = "D83DDD11";
            _dict["D83DDD12"] = "D83DDD12";
            _dict["D83DDD13"] = "D83DDD13";
            _dict["D83DDD14"] = "D83DDD14";
            _dict["D83DDD15"] = "D83DDD15";
            _dict["D83DDD16"] = "D83DDD16";
            _dict["D83DDD17"] = "D83DDD17";
            _dict["D83DDD18"] = "D83DDD18";
            _dict["D83DDD19"] = "D83DDD19";
            _dict["D83DDD1A"] = "D83DDD1A";
            _dict["D83DDD1B"] = "D83DDD1B";
            _dict["D83DDD1C"] = "D83DDD1C";
            _dict["D83DDD1D"] = "D83DDD1D";
            _dict["D83DDD1E"] = "D83DDD1E";
            _dict["D83DDD1F"] = "D83DDD1F";
            _dict["D83DDD20"] = "D83DDD20";
            _dict["D83DDD21"] = "D83DDD21";
            _dict["D83DDD22"] = "D83DDD22";
            _dict["D83DDD23"] = "D83DDD23";
            _dict["D83DDD24"] = "D83DDD24";
            _dict["D83DDD25"] = "D83DDD25";
            _dict["D83DDD26"] = "D83DDD26";
            _dict["D83DDD27"] = "D83DDD27";
            _dict["D83DDD28"] = "D83DDD28";
            _dict["D83DDD29"] = "D83DDD29";
            _dict["D83DDD2A"] = "D83DDD2A";
            _dict["D83DDD2B"] = "D83DDD2B";
            _dict["D83DDD2C"] = "D83DDD2C";
            _dict["D83DDD2D"] = "D83DDD2D";
            _dict["D83DDD2E"] = "D83DDD2E";
            _dict["D83DDD2F"] = "D83DDD2F";
            _dict["D83DDD30"] = "D83DDD30";
            _dict["D83DDD31"] = "D83DDD31";
            _dict["D83DDD32"] = "D83DDD32";
            _dict["D83DDD33"] = "D83DDD33";
            _dict["D83DDD34"] = "D83DDD34";
            _dict["D83DDD35"] = "D83DDD35";
            _dict["D83DDD36"] = "D83DDD36";
            _dict["D83DDD37"] = "D83DDD37";
            _dict["D83DDD38"] = "D83DDD38";
            _dict["D83DDD39"] = "D83DDD39";
            _dict["D83DDD3A"] = "D83DDD3A";
            _dict["D83DDD3B"] = "D83DDD3B";
            _dict["D83DDD3C"] = "D83DDD3C";
            _dict["D83DDD3D"] = "D83DDD3D";
            _dict["D83DDD50"] = "D83DDD50";
            _dict["D83DDD51"] = "D83DDD51";
            _dict["D83DDD52"] = "D83DDD52";
            _dict["D83DDD53"] = "D83DDD53";
            _dict["D83DDD54"] = "D83DDD54";
            _dict["D83DDD55"] = "D83DDD55";
            _dict["D83DDD56"] = "D83DDD56";
            _dict["D83DDD57"] = "D83DDD57";
            _dict["D83DDD58"] = "D83DDD58";
            _dict["D83DDD59"] = "D83DDD59";
            _dict["D83DDD5A"] = "D83DDD5A";
            _dict["D83DDD5B"] = "D83DDD5B";
            _dict["D83DDD5C"] = "D83DDD5C";
            _dict["D83DDD5D"] = "D83DDD5D";
            _dict["D83DDD5E"] = "D83DDD5E";
            _dict["D83DDD5F"] = "D83DDD5F";
            _dict["D83DDD60"] = "D83DDD60";
            _dict["D83DDD61"] = "D83DDD61";
            _dict["D83DDD62"] = "D83DDD62";
            _dict["D83DDD63"] = "D83DDD63";
            _dict["D83DDD64"] = "D83DDD64";
            _dict["D83DDD65"] = "D83DDD65";
            _dict["D83DDD66"] = "D83DDD66";
            _dict["D83DDD67"] = "D83DDD67";
            _dict["D83DDDFB"] = "D83DDDFB";
            _dict["D83DDDFC"] = "D83DDDFC";
            _dict["D83DDDFD"] = "D83DDDFD";
            _dict["D83DDDFE"] = "D83DDDFE";
            _dict["D83DDDFF"] = "D83DDDFF";
            _dict["D83DDE00"] = "D83DDE00";
            _dict["D83DDE01"] = "D83DDE01";
            _dict["D83DDE02"] = "D83DDE02";
            _dict["D83DDE03"] = "D83DDE03";
            _dict["D83DDE04"] = "D83DDE04";
            _dict["D83DDE05"] = "D83DDE05";
            _dict["D83DDE06"] = "D83DDE06";
            _dict["D83DDE07"] = "D83DDE07";
            _dict["D83DDE08"] = "D83DDE08";
            _dict["D83DDE09"] = "D83DDE09";
            _dict["D83DDE0A"] = "D83DDE0A";
            _dict["D83DDE0B"] = "D83DDE0B";
            _dict["D83DDE0C"] = "D83DDE0C";
            _dict["D83DDE0D"] = "D83DDE0D";
            _dict["D83DDE0E"] = "D83DDE0E";
            _dict["D83DDE0F"] = "D83DDE0F";
            _dict["D83DDE10"] = "D83DDE10";
            _dict["D83DDE11"] = "D83DDE11";
            _dict["D83DDE12"] = "D83DDE12";
            _dict["D83DDE13"] = "D83DDE13";
            _dict["D83DDE14"] = "D83DDE14";
            _dict["D83DDE15"] = "D83DDE15";
            _dict["D83DDE16"] = "D83DDE16";
            _dict["D83DDE17"] = "D83DDE17";
            _dict["D83DDE18"] = "D83DDE18";
            _dict["D83DDE19"] = "D83DDE19";
            _dict["D83DDE1A"] = "D83DDE1A";
            _dict["D83DDE1B"] = "D83DDE1B";
            _dict["D83DDE1C"] = "D83DDE1C";
            _dict["D83DDE1D"] = "D83DDE1D";
            _dict["D83DDE1E"] = "D83DDE1E";
            _dict["D83DDE1F"] = "D83DDE1F";
            _dict["D83DDE20"] = "D83DDE20";
            _dict["D83DDE21"] = "D83DDE21";
            _dict["D83DDE22"] = "D83DDE22";
            _dict["D83DDE23"] = "D83DDE23";
            _dict["D83DDE24"] = "D83DDE24";
            _dict["D83DDE25"] = "D83DDE25";
            _dict["D83DDE26"] = "D83DDE26";
            _dict["D83DDE27"] = "D83DDE27";
            _dict["D83DDE28"] = "D83DDE28";
            _dict["D83DDE29"] = "D83DDE29";
            _dict["D83DDE2A"] = "D83DDE2A";
            _dict["D83DDE2B"] = "D83DDE2B";
            _dict["D83DDE2C"] = "D83DDE2C";
            _dict["D83DDE2D"] = "D83DDE2D";
            _dict["D83DDE2E"] = "D83DDE2E";
            _dict["D83DDE2F"] = "D83DDE2F";
            _dict["D83DDE30"] = "D83DDE30";
            _dict["D83DDE31"] = "D83DDE31";
            _dict["D83DDE32"] = "D83DDE32";
            _dict["D83DDE33"] = "D83DDE33";
            _dict["D83DDE34"] = "D83DDE34";
            _dict["D83DDE35"] = "D83DDE35";
            _dict["D83DDE36"] = "D83DDE36";
            _dict["D83DDE37"] = "D83DDE37";
            _dict["D83DDE38"] = "D83DDE38";
            _dict["D83DDE39"] = "D83DDE39";
            _dict["D83DDE3A"] = "D83DDE3A";
            _dict["D83DDE3B"] = "D83DDE3B";
            _dict["D83DDE3C"] = "D83DDE3C";
            _dict["D83DDE3D"] = "D83DDE3D";
            _dict["D83DDE3E"] = "D83DDE3E";
            _dict["D83DDE3F"] = "D83DDE3F";
            _dict["D83DDE40"] = "D83DDE40";
            _dict["D83DDE45"] = "D83DDE45";
            _dict["D83DDE46"] = "D83DDE46";
            _dict["D83DDE47"] = "D83DDE47";
            _dict["D83DDE48"] = "D83DDE48";
            _dict["D83DDE49"] = "D83DDE49";
            _dict["D83DDE4A"] = "D83DDE4A";
            _dict["D83DDE4B"] = "D83DDE4B";
            _dict["D83DDE4C"] = "D83DDE4C";
            _dict["D83DDE4D"] = "D83DDE4D";
            _dict["D83DDE4E"] = "D83DDE4E";
            _dict["D83DDE4F"] = "D83DDE4F";
            _dict["D83DDE80"] = "D83DDE80";
            _dict["D83DDE81"] = "D83DDE81";
            _dict["D83DDE82"] = "D83DDE82";
            _dict["D83DDE83"] = "D83DDE83";
            _dict["D83DDE84"] = "D83DDE84";
            _dict["D83DDE85"] = "D83DDE85";
            _dict["D83DDE86"] = "D83DDE86";
            _dict["D83DDE87"] = "D83DDE87";
            _dict["D83DDE88"] = "D83DDE88";
            _dict["D83DDE89"] = "D83DDE89";
            _dict["D83DDE8A"] = "D83DDE8A";
            _dict["D83DDE8B"] = "D83DDE8B";
            _dict["D83DDE8C"] = "D83DDE8C";
            _dict["D83DDE8D"] = "D83DDE8D";
            _dict["D83DDE8E"] = "D83DDE8E";
            _dict["D83DDE8F"] = "D83DDE8F";
            _dict["D83DDE90"] = "D83DDE90";
            _dict["D83DDE91"] = "D83DDE91";
            _dict["D83DDE92"] = "D83DDE92";
            _dict["D83DDE93"] = "D83DDE93";
            _dict["D83DDE94"] = "D83DDE94";
            _dict["D83DDE95"] = "D83DDE95";
            _dict["D83DDE96"] = "D83DDE96";
            _dict["D83DDE97"] = "D83DDE97";
            _dict["D83DDE98"] = "D83DDE98";
            _dict["D83DDE99"] = "D83DDE99";
            _dict["D83DDE9A"] = "D83DDE9A";
            _dict["D83DDE9B"] = "D83DDE9B";
            _dict["D83DDE9C"] = "D83DDE9C";
            _dict["D83DDE9D"] = "D83DDE9D";
            _dict["D83DDE9E"] = "D83DDE9E";
            _dict["D83DDE9F"] = "D83DDE9F";
            _dict["D83DDEA0"] = "D83DDEA0";
            _dict["D83DDEA1"] = "D83DDEA1";
            _dict["D83DDEA2"] = "D83DDEA2";
            _dict["D83DDEA3"] = "D83DDEA3";
            _dict["D83DDEA4"] = "D83DDEA4";
            _dict["D83DDEA5"] = "D83DDEA5";
            _dict["D83DDEA6"] = "D83DDEA6";
            _dict["D83DDEA7"] = "D83DDEA7";
            _dict["D83DDEA8"] = "D83DDEA8";
            _dict["D83DDEA9"] = "D83DDEA9";
            _dict["D83DDEAA"] = "D83DDEAA";
            _dict["D83DDEAB"] = "D83DDEAB";
            _dict["D83DDEAC"] = "D83DDEAC";
            _dict["D83DDEAD"] = "D83DDEAD";
            _dict["D83DDEAE"] = "D83DDEAE";
            _dict["D83DDEAF"] = "D83DDEAF";
            _dict["D83DDEB0"] = "D83DDEB0";
            _dict["D83DDEB1"] = "D83DDEB1";
            _dict["D83DDEB2"] = "D83DDEB2";
            _dict["D83DDEB3"] = "D83DDEB3";
            _dict["D83DDEB4"] = "D83DDEB4";
            _dict["D83DDEB5"] = "D83DDEB5";
            _dict["D83DDEB6"] = "D83DDEB6";
            _dict["D83DDEB7"] = "D83DDEB7";
            _dict["D83DDEB8"] = "D83DDEB8";
            _dict["D83DDEB9"] = "D83DDEB9";
            _dict["D83DDEBA"] = "D83DDEBA";
            _dict["D83DDEBB"] = "D83DDEBB";
            _dict["D83DDEBC"] = "D83DDEBC";
            _dict["D83DDEBD"] = "D83DDEBD";
            _dict["D83DDEBE"] = "D83DDEBE";
            _dict["D83DDEBF"] = "D83DDEBF";
            _dict["D83DDEC0"] = "D83DDEC0";
            _dict["D83DDEC1"] = "D83DDEC1";
            _dict["D83DDEC2"] = "D83DDEC2";
            _dict["D83DDEC3"] = "D83DDEC3";
            _dict["D83DDEC4"] = "D83DDEC4";
            _dict["D83DDEC5"] = "D83DDEC5";
        }
    }

    public static class BrowserNavigationService
    {
        private static double _fontScaleFactor = 1.0;

        public static double FontScaleFactor
        {
            get { return _fontScaleFactor; }
            set { _fontScaleFactor = value; }
        }

        // http://daringfireball.net/2010/07/improved_regex_for_matching_urls
        private static readonly Regex RE_URL = new Regex(@"(?i)\b(((?:https?://|www\d{0,3}[.]|[a-z0-9.\-]+[.][a-z]{2,4}/)(?:[^\s()<>]+|\(([^\s()<>]+|(\([^\s()<>]+\)))*\))+(?:\(([^\s()<>]+|(\([^\s()<>]+\)))*\)|[^\s`!()\[\]{};:'"".,<>?«»“”‘’]))|([a-z0-9.\-]+(\.ru|\.com|\.net|\.org|\.us|\.it|\.co\.uk)(?![a-z0-9]))|([a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]*[a-zA-Z0-9-]+))");

        public static readonly Regex UserMentionRegex = new Regex(@"\[id\d+.*?\|.*?\]");
        public static readonly Regex GroupMentionRegex = new Regex(@"\[club\d+.*?\|.*?\]");


        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
            "Text",
            typeof(string),
            typeof(BrowserNavigationService),
            new PropertyMetadata(null, OnTextChanged)
        );

        public static string GetText(DependencyObject d)
        { return d.GetValue(TextProperty) as string; }

        public static void SetText(DependencyObject d, string value)
        { d.SetValue(TextProperty, value); }

        // Fetch run with PhoneTextNormalStyle
        public static Run GetRunWithStyle(string text, RichTextBox richTextBox)
        {
            var run = new Run();

            run.FontFamily = richTextBox.FontFamily;
            //run.FontSize = richTextBox.FontSize * TextScaleFactor;
            run.Foreground = richTextBox.Foreground;// (Brush)Application.Current.Resources["PhoneForegroundBrush"];
            run.Text = text;

            return run;
        }

        public static event EventHandler<TelegramEventArgs> ResolveUsername;

        private static void RaiseTelegramNavigated(TelegramEventArgs e)
        {
            var handler = ResolveUsername;
            if (handler != null) handler(null, e);
        }

        public static event EventHandler<TelegramHashtagEventArgs> SearchHashtag;

        private static void RaiseSearchHashtag(TelegramHashtagEventArgs e)
        {
            var handler = SearchHashtag;
            if (handler != null) handler(null, e);
        }

        public static event EventHandler<TelegramMentionEventArgs> MentionNavigated;

        private static void RaiseMentionNavigated(TelegramMentionEventArgs e)
        {
            var handler = MentionNavigated;
            if (handler != null) handler(null, e);
        }

        public static Hyperlink GetHyperlink(string text, string link, Action<object, string> action, Brush foreground)
        {
            var hyperlink = new Hyperlink();
            hyperlink.Inlines.Add(new Run { Text = text, Foreground = foreground });
            hyperlink.NavigateUri = new Uri(link, UriKind.RelativeOrAbsolute);
            hyperlink.Click += (sender, args) => action(hyperlink, link);
            return hyperlink;
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var text_block = d as RichTextBox;
            if (text_block == null)
                return;

            text_block.Blocks.Clear();
            Paragraph par = new Paragraph();


            var new_text = (string)e.NewValue;


            if (string.IsNullOrEmpty(new_text))
                return;

            //new_text = PreprocessTextForGroupBoardMentions(new_text);

            var splitData = ParseText(new_text);

            foreach (var splStr in splitData)
            {
                var innerSplit = splStr.Split('\b');

                if (innerSplit.Length == 1)
                {
                    AddRawText(text_block, par, innerSplit[0]);
                }
                else if (innerSplit.Length > 1)
                {
                    var hyp = GetHyperlink(
                           innerSplit[1],
                           innerSplit[0],
                           (h, navstr) =>
                           {
                               NavigateOnHyperlink(navstr);
                           },
                           text_block.Foreground);

                    par.Inlines.Add(hyp);
                }
            }
            //var dateRun = new Run {Text = "1:23 1.07.2014"};
            //dateRun.
            //par.Inlines.Add(dateRun);

            text_block.Blocks.Add(par);
        }

        private static void AddRawText(RichTextBox text_block, Paragraph par, string raw_text)
        {

            var textEnumerator = StringInfo.GetTextElementEnumerator(raw_text);
            bool cont = false;


            //  DebugWriteUnicode(raw_text);

            StringBuilder sb = new StringBuilder();

            // Note: Begins at element -1 (none).
            cont = textEnumerator.MoveNext();
            while (cont)
            {
                var text = textEnumerator.GetTextElement();

                var bytes = Encoding.BigEndianUnicode.GetBytes(text);

                var bytesStr = ConvertToHexString(bytes);

                if (_flagsPrefixes.Contains(bytesStr) && textEnumerator.MoveNext())
                {
                    var text2 = textEnumerator.GetTextElement();

                    var bytes2 = Encoding.BigEndianUnicode.GetBytes(text2);

                    var bytesStr2 = ConvertToHexString(bytes2);

                    bytesStr += bytesStr2;

                    text += text2;
                }

                if (Emoji.Dict.ContainsKey(bytesStr))
                {
                    var sbStr = sb.ToString();
                    sb = sb.Clear();
                    if (sbStr != string.Empty)
                    {
                        par.Inlines.Add(GetRunWithStyle(sbStr, text_block));
                    }

                    par.Inlines.Add(GetImage(bytesStr));
                }

                else
                {
                    sb = sb.Append(text);
                }

                cont = textEnumerator.MoveNext();

            }

            var sbStrLast = sb.ToString();

            if (sbStrLast != string.Empty)
            {
                par.Inlines.Add(GetRunWithStyle(sbStrLast, text_block));
            }
        }

        private static List<string> _flagsPrefixes =
        new List<string>{"D83CDDE8",
            "D83CDDE9",
            "D83CDDEA",
            "D83CDDEB",
            "D83CDDEC",
            "D83CDDEE",
            "D83CDDEF",
            "D83CDDF0",
            "D83CDDF7",
            "D83CDDFA"};
















        //public static void DebugWriteUnicode(string input)
        //{
        //    var res = "";
        //    for (var i = 0; i < input.Length; i += char.IsSurrogatePair(input, i) ? 2 : 1)
        //    {
        //        var codepoint = char.ConvertToUtf32(input, i);

        //        res += string.Format("U+{0:X4}", codepoint);
        //    }

        //    Debug.WriteLine(res);
        //}
        private static string ConvertToHexString(byte[] bytes)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb = sb.Append(Convert.ToString(bytes[i], 16).PadLeft(2, '0'));
            }

            return sb.ToString().ToUpperInvariant();
        }

        private static InlineUIContainer GetImage(string name)
        {
            var image = new Image();
            image.Source = new BitmapImage(new Uri(string.Format("/Assets/Emoji/Separated/{0}.png", name), UriKind.RelativeOrAbsolute));
            image.Height = 27 * FontScaleFactor;
            image.Width = 27 * FontScaleFactor;
            image.Margin = new Thickness(0, 5, 0, -5);
            var container = new InlineUIContainer();
            
            container.Child = image;

            return container;
        }

        private static void NavigateOnHyperlink(string navstr)
        {
            if (string.IsNullOrEmpty(navstr)) return;

            if (navstr.StartsWith("tlg://?action=mention"))
            {
                var mentionIndex = navstr.IndexOf('@');

                if (mentionIndex != -1)
                {
                    var mention = navstr.Substring(mentionIndex);

                    RaiseMentionNavigated(new TelegramMentionEventArgs { Mention = mention });
                }
            }
            else if (navstr.StartsWith("tlg://?action=search"))
            {
                var hashtagIndex = navstr.IndexOf('#');

                if (hashtagIndex != -1)
                {
                    var hashtag = navstr.Substring(hashtagIndex);

                    RaiseSearchHashtag(new TelegramHashtagEventArgs{ Hashtag = hashtag });
                }
            }
            else if (!navstr.Contains("@"))
            {
                if (navstr.ToLowerInvariant().Contains("telegram.me"))
                {
                    RaiseTelegramNavigated(new TelegramEventArgs{Uri = navstr});
                }
                else
                {
                    var task = new WebBrowserTask();
                    task.URL = navstr;
                    task.Show();
                }
            }
            else
            {
                EmailComposeTask emailComposeTask = new EmailComposeTask();

                if (navstr.StartsWith("http://"))
                {
                    navstr = navstr.Remove(0, 7);
                }

                emailComposeTask.To = navstr;

                emailComposeTask.Show();
            }
        }


        public static string PreprocessTextForGroupBoardMentions(string s)
        {
            s = Regex.Replace(s, @"\[(id|club)(\d+):bp\-(\d+)_(\d+)\|([^\]]+)\]", "[$1$2|$5]");
            return s;
        }

        private static bool IsValidUsernameSymbol(char symbol)
        {
            if ((symbol >= 'a' && symbol <= 'z')
                || (symbol >= 'A' && symbol <= 'Z')
                || (symbol >= '0' && symbol <= '9')
                || symbol == '_')
            {
                return true;
            }

            return false;
        }

        private static bool IsValidUsername(string username)
        {
            if (username.Length <= 5)
            {
                return false;
            }

            if (username.Length > 32)
            {
                return false;
            }

            if (username[0] != '@')
            {
                return false;
            }

            for (var i = 1; i < username.Length; i++)
            {
                if (!IsValidUsernameSymbol(username[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static List<string> ParseText(string html)
        {
            html = html.Replace("\n", " \n ");
            var rx = new Regex("(https?:\\/\\/)?(([A-Za-zА-Яа-яЁё0-9@][A-Za-zА-Яа-яЁё0-9@\\-_\\.]*[A-Za-zА-Яа-яЁё0-9@])(\\/([A-Za-zА-Яа-я0-9@\\-_#%&?+\\/\\.=;:~]*[^\\.\\,;\\(\\)\\?<\\&\\s:])?)?)", RegexOptions.IgnoreCase);
            html = rx.Replace(html, delegate(Match m)
            {
                var full = m.Value;

                if (full.IndexOf('@') == 0 && IsValidUsername(full))
                    return string.Format("\atlg://?action=mention&q={0}\b{1}\a", full, full);
                
                var protocol = (m.Groups.Count > 1) ? m.Groups[1].Value : "http://";
                if (protocol == string.Empty) protocol = "http://";
                var url = (m.Groups.Count > 2) ? m.Groups[2].Value : string.Empty;
                var domain = (m.Groups.Count > 3) ? m.Groups[3].Value : string.Empty;
                if (domain.IndexOf(".") == -1 || domain.IndexOf("..") != -1) return full;
                var topDomain = domain.Split('.').LastOrDefault();
                if (topDomain.Length > 5 ||
                    !("guru,info,name,aero,arpa,coop,museum,mobi,travel,xxx,asia,biz,com,net,org,gov,mil,edu,int,tel,ac,ad,ae,af,ag,ai,al,am,an,ao,aq,ar,as,at,au,aw,az,ba,bb,bd,be,bf,bg,bh,bi,bj,bm,bn,bo,br,bs,bt,bv,bw,by,bz,ca,cc,cd,cf,cg,ch,ci,ck,cl,cm,cn,co,cr,cu,cv,cx,cy,cz,de,dj,dk,dm,do,dz,ec,ee,eg,eh,er,es,et,eu,fi,fj,fk,fm,fo,fr,ga,gd,ge,gf,gg,gh,gi,gl,gm,gn,gp,gq,gr,gs,gt,gu,gw,gy,hk,hm,hn,hr,ht,hu,id,ie,il,im,in,io,iq,ir,is,it,je,jm,jo,jp,ke,kg,kh,ki,km,kn,kp,kr,kw,ky,kz,la,lb,lc,li,lk,lr,ls,lt,lu,lv,ly,ma,mc,md,me,mg,mh,mk,ml,mm,mn,mo,mp,mq,mr,ms,mt,mu,mv,mw,mx,my,mz,na,nc,ne,nf,ng,ni,nl,no,np,nr,nu,nz,om,pa,pe,pf,pg,ph,pk,pl,pm,pn,pr,ps,pt,pw,py,qa,re,ro,ru,rw,sa,sb,sc,sd,se,sg,sh,si,sj,sk,sl,sm,sn,so,sr,st,su,sv,sy,sz,tc,td,tf,tg,th,tj,tk,tl,tm,tn,to,tp,tr,tt,tv,tw,tz,ua,ug,uk,um,us,uy,uz,va,vc,ve,vg,vi,vn,vu,wf,ws,ye,yt,yu,za,zm,zw,рф,cat,pro"
                        .Split(',').Contains(topDomain))) return full;
                if (full.IndexOf('@') != -1) return "\amailto:" + full + "\b" + full + "\a";
                

                full = HttpUtility.UrlDecode(full);
                if (full.Length > 55) full = full.Substring(0, 53) + "..";

                return string.Format("\a{0}\b{1}\a", (protocol + url), full);
            }).ReplaceByRegex("(^|\\s)#[\\w@\\.]+", " \atlg://?action=search&q=$0\b$0\a"); ;
            html = html.Replace("\n ", "\n").Replace(" \n", "\n");

            if (html.StartsWith(" ")) html = html.Remove(0, 1);
            var blocks = html.Split('\a');
            return blocks.ToList();
        }

    }

    public static class StringExtensions
    {
        public static string ReplaceByRegex(this string str, string regexStr, string replace)
        {
            var regex = new Regex(regexStr);

            var result = regex.Replace(str, replace);

            return result;
        }
    }

    public class TelegramEventArgs : EventArgs
    {
        public string Uri { get; set; }
    }

    public class TelegramHashtagEventArgs : EventArgs
    {
        public string Hashtag { get; set; }
    }

    public class TelegramMentionEventArgs : EventArgs
    {
        public string Mention { get; set; }
    }
}

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Web;
//using BarcodeLib;
//using BarcodeLib;
using Spire.Barcode;
using PQT.Domain.Helpers;

//namespace PQT.Web.Infrastructure.Helpers
//{
//    public class BarCodeHelper
//    {
//        public static string[] GetBarCodeString(string filepath)
//        {
//            return new string[5];
//        }

//        public static string GenerateBarcode(string code)
//        {
//            return null;
//        }
//    }
//}


///////backup

namespace PQT.Web.Infrastructure.Helpers
{
    public class BarCodeHelper
    {
        public static string[] GetBarCodeString(string filepath)
        {
            string[] barcodes = BarcodeScanner.Scan(ConvertHelper.ConvertToImage(filepath),
                                                    BarCodeType.Code39);
            return barcodes;
        }

        //public static string GenerateBarcode(string code)
        //{
        //    string barcodeImageName = Guid.NewGuid() + ".jpg";

        //    var barcode = new Barcode();
        //    const int width = 500;
        //    const int height = 50;
        //    barcode.Alignment = AlignmentPositions.CENTER;

        //    #region barcode alignment

        //    //switch (cbBarcodeAlign.SelectedItem.ToString().Trim().ToLower())
        //    //{
        //    //    case "left": b.Alignment = BarcodeLib.AlignmentPositions.LEFT; break;
        //    //    case "right": b.Alignment = BarcodeLib.AlignmentPositions.RIGHT; break;
        //    //    default: b.Alignment = BarcodeLib.AlignmentPositions.CENTER; break;
        //    //}//switch

        //    #endregion

        //    const TYPE type = TYPE.CODE39;

        //    #region Barcodetype

        //    //switch (cbEncodeType.SelectedItem.ToString().Trim())
        //    //{
        //    //    case "UPC-A": type = BarcodeLib.TYPE.UPCA; break;
        //    //    case "UPC-E": type = BarcodeLib.TYPE.UPCE; break;
        //    //    case "UPC 2 Digit Ext.": type = BarcodeLib.TYPE.UPC_SUPPLEMENTAL_2DIGIT; break;
        //    //    case "UPC 5 Digit Ext.": type = BarcodeLib.TYPE.UPC_SUPPLEMENTAL_5DIGIT; break;
        //    //    case "EAN-13": type = BarcodeLib.TYPE.EAN13; break;
        //    //    case "JAN-13": type = BarcodeLib.TYPE.JAN13; break;
        //    //    case "EAN-8": type = BarcodeLib.TYPE.EAN8; break;
        //    //    case "ITF-14": type = BarcodeLib.TYPE.ITF14; break;
        //    //    case "Codabar": type = BarcodeLib.TYPE.Codabar; break;
        //    //    case "PostNet": type = BarcodeLib.TYPE.PostNet; break;
        //    //    case "Bookland/ISBN": type = BarcodeLib.TYPE.BOOKLAND; break;
        //    //    case "Code 11": type = BarcodeLib.TYPE.CODE11; break;
        //    //    case "Code 39": type = BarcodeLib.TYPE.CODE39; break;
        //    //    case "Code 39 Extended": type = BarcodeLib.TYPE.CODE39Extended; break;
        //    //    case "Code 93": type = BarcodeLib.TYPE.CODE93; break;
        //    //    case "LOGMARS": type = BarcodeLib.TYPE.LOGMARS; break;
        //    //    case "MSI": type = BarcodeLib.TYPE.MSI_Mod10; break;
        //    //    case "Interleaved 2 of 5": type = BarcodeLib.TYPE.Interleaved2of5; break;
        //    //    case "Standard 2 of 5": type = BarcodeLib.TYPE.Standard2of5; break;
        //    //    case "Code 128": type = BarcodeLib.TYPE.CODE128; break;
        //    //    case "Code 128-A": type = BarcodeLib.TYPE.CODE128A; break;
        //    //    case "Code 128-B": type = BarcodeLib.TYPE.CODE128B; break;
        //    //    case "Code 128-C": type = BarcodeLib.TYPE.CODE128C; break;
        //    //    case "Telepen": type = BarcodeLib.TYPE.TELEPEN; break;
        //    //    case "FIM": type = BarcodeLib.TYPE.FIM; break;
        //    //    default: MessageBox.Show("Please specify the encoding type."); break;
        //    //}//switch

        //    #endregion

        //    try
        //    {
        //        barcode.IncludeLabel = true;

        //        barcode.RotateFlipType = RotateFlipType.RotateNoneFlipNone;

        //        barcode.LabelPosition = LabelPositions.BOTTOMCENTER;
        //        //label alignment and position

        //        #region label alignment and position

        //        //switch (this.cbLabelLocation.SelectedItem.ToString().Trim().ToUpper())
        //        //{
        //        //    case "BOTTOMLEFT": b.LabelPosition = BarcodeLib.LabelPositions.BOTTOMLEFT; break;
        //        //    case "BOTTOMRIGHT": b.LabelPosition = BarcodeLib.LabelPositions.BOTTOMRIGHT; break;
        //        //    case "TOPCENTER": b.LabelPosition = BarcodeLib.LabelPositions.TOPCENTER; break;
        //        //    case "TOPLEFT": b.LabelPosition = BarcodeLib.LabelPositions.TOPLEFT; break;
        //        //    case "TOPRIGHT": b.LabelPosition = BarcodeLib.LabelPositions.TOPRIGHT; break;
        //        //    default: b.LabelPosition = BarcodeLib.LabelPositions.BOTTOMCENTER; break;
        //        //}//switch

        //        #endregion

        //        //===== Encoding performed here =====
        //        Image image = barcode.Encode(type, code, Color.Black, Color.White, width, height);
        //        string location = HttpContext.Current.Server.MapPath("~/attachments/" + barcodeImageName);
        //        image.Save(location, ImageFormat.Jpeg);
        //        //===================================

        //        return barcodeImageName;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}
    }
}
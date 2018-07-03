using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OSGeo.GDAL;
using UnityEngine;

public class RasterReader
{
    /// <summary>
    /// 
    /// </summary>
    public struct RasterInfo
    {
        public string filepath;
        public int rasterSizeX;
        public int rasterSizeY;
        public int bandsCount;
        public DataType bandType;
        public int bandSizeX;
        public int bandSizeY;
        public int blockSizeX;
        public int blockSizeY;
        public float min;
        public float max;
        public float mean;
        public float stdDev;
        public float overviewCount;
        //public string errorMsgs;

        public override string ToString()
        {
            return string.Format(
                "File Path: {0}\n" +
                "Raster Size X: {1}\n" +
                "Raster Size Y: {2}\n" +
                "Bands Count  : {3}\n" +
                "Band Type    : {4}\n" +
                "Band Size X  : {5}\n" +
                "Band Size Y  : {6}\n" +
                "Block Size X : {7}\n" +
                "Block Size Y : {8}\n" +
                "Min   : {9:N2}\n" +
                "Max   : {10:N2}\n" +
                "Mean  : {11:N2}\n" +
                "StdDev: {12:N2}\n" +
                "Overview Count: {13}",
                filepath, rasterSizeX, rasterSizeY, bandsCount,
                bandType.ToString(), bandSizeX, bandSizeY,
                blockSizeX, blockSizeY,
                min, max, mean, stdDev, overviewCount);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="rasterInfo"></param>
    /// <returns></returns>
    public static float[] ReadTIFF(string path, out RasterInfo rasterInfo)
    {
        Gdal.AllRegister();

        Dataset dataset = Gdal.Open(path, Access.GA_ReadOnly);
        if (dataset == null)
        {
            Debug.LogError("Unable to load dataset at \"" + path + "\"");
            rasterInfo = default(RasterInfo);
            return null;
        }

        int rasterSizeX = dataset.RasterXSize;
        int rasterSizey = dataset.RasterYSize;
        int blockSizeX, blockSizeY;
        double min, max, mean, stdDev;

        Band band = dataset.GetRasterBand(1);
        band.GetBlockSize(out blockSizeX, out blockSizeY);

        Debug.Log(string.Format("Driver: {0}/{1}", dataset.GetDriver().ShortName, dataset.GetDriver().LongName));
        Debug.Log(string.Format("Size is {0} x {1} x {2}", dataset.RasterXSize, dataset.RasterYSize, dataset.RasterCount));
        Debug.Log(string.Format("Band Type={0}, xSize={1}, ySize={2}, BlockSizeX={3}, BlockSizeY={4}",
            Gdal.GetDataTypeName(band.DataType), band.XSize, band.YSize, blockSizeX, blockSizeY));

        { // not used
            // metadata
            //foreach (string meta in dataset.GetMetadata(string.Empty))
            //    Debug.Log(meta);
            //int hasMin, hasMax;
            //band.GetMinimum(out min, out hasMin);
            //band.GetMaximum(out max, out hasMax);
            //if (hasMin == 1 && hasMax == 1)
            //    Debug.Log("It HAS :: " + string.Format("Min={0:N2}, Max={1:N2}", min, max));
            //else
        }

        // statistics
        CPLErr err = band.ComputeStatistics(false, out min, out max, out mean, out stdDev, null, null);
        if (err == CPLErr.CE_None)
            Debug.Log(string.Format("Min={0:N2}, Max={1:N2}, Mean={1:N2}, StdDev={1:N2}", min, max, mean, stdDev));
        else
            Debug.Log("Error computing statistics :: " + err.ToString());

        // overviews
        if (band.GetOverviewCount() > 0)
            Debug.Log("Band overview count: " + band.GetOverviewCount());
        else
            Debug.Log("No overviews");

        // colortable
        ColorTable colortable = band.GetRasterColorTable();
        if (colortable != null)
            Debug.Log("Band has a ColorTable with " + band.GetRasterColorTable().GetCount() + " entries");
        else
            Debug.Log("No ColorTable");

        // extract data
        float[] rasterData = new float[rasterSizeX * rasterSizey];
        band.ReadRaster(0, 0, rasterSizeX, rasterSizey, rasterData, rasterSizeX, rasterSizey, 0, 0);

        {// print sample data
         //StringBuilder sb = new StringBuilder();
         //sb.Append("RasterData: " + rasterData.Length.ToString() + " :: ");
         //for (int i = 0; i < 50; i++)
         //    sb.Append(rasterData[i].ToString() + " | ");
         //Debug.Log(sb.ToString());
        }

        rasterInfo = new RasterInfo()
        {
            filepath = path,
            rasterSizeX = dataset.RasterXSize,
            rasterSizeY = dataset.RasterYSize,
            bandsCount = dataset.RasterCount,
            bandType = band.DataType,
            bandSizeX = band.XSize,
            bandSizeY = band.YSize,
            blockSizeX = blockSizeX,
            blockSizeY = blockSizeY,
            min = (float)min,
            max = (float)max,
            mean = (float)mean,
            stdDev = (float)stdDev,
            overviewCount = band.GetOverviewCount()
        };

        return rasterData;
    }

    //public static bool WriteTIFF(float[] data, string path, int sizeX, int sizeY, int bands = 1, DataType dataType = DataType.GDT_Float32)
    //{
    //    if(data == null || data.Length != (sizeX * sizeY * bands))
    //    {
    //        Debug.LogError("Error writting to file :: Invalid data.");
    //        return false;
    //    }

    //    Gdal.AllRegister();
    //    Driver driver = Gdal.GetDriverByName("GTiff");

    //    string[] options = { "BLOCKXSIZE=256", "BLOCKYSIZE=256" }; ///////////////////////

    //    Dataset dataset = driver.Create(path, sizeX, sizeY, bands, dataType, options);

    //    if(dataset == null)
    //    {
    //        Debug.Log("Error writting to file :: Failed to create the dataset.");
    //        return false;
    //    }

    //    Band band = dataset.GetRasterBand(1);

    //    CPLErr err = band.WriteRaster(0, 0, sizeX, sizeY, data, sizeX, sizeY, 0, 0);

    //    dataset.Dispose();
    //    driver.Dispose();

    //    if (err == CPLErr.CE_None)
    //    {
    //        Debug.Log("Data written to file at \"" + path + "\". " + data.Length);
    //        return true;
    //    }
    //    else
    //    {
    //        Debug.Log("Error writting to file :: " + err.ToString());
    //        return false;
    //    }
    //}
}

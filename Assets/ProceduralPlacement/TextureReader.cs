using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OSGeo.GDAL;
using UnityEngine;

public class TextureReader {

	
    public static float[] ReadTIFF(string path)
    {
        Gdal.AllRegister();

        Dataset dataset = Gdal.Open(path, Access.GA_ReadOnly);

        if(dataset == null)
        {
            Debug.LogError("Unable to load dataset at \"" + path + "\"");
            return null;
        }

        Debug.Log(string.Format("Driver: {0}/{1}", dataset.GetDriver().ShortName, dataset.GetDriver().LongName));
        Debug.Log(string.Format("Size is {0} x {1} x {2}", dataset.RasterXSize, dataset.RasterYSize, dataset.RasterCount));

        foreach(string meta in dataset.GetMetadata(string.Empty))
        {
            Debug.Log(meta);
        }

        Band band = dataset.GetRasterBand(1);
        int bsx, bsy;
        band.GetBlockSize(out bsx, out bsy);

        Debug.Log(string.Format("Band Type={0}, xSize={1}, ySize={2}, BlockSizeX={3}, BlockSizeY={4}", Gdal.GetDataTypeName(band.DataType),
            band.XSize, band.YSize, bsx, bsy));

        double min, max;
        int hasMin, hasMax;
        band.GetMinimum(out min, out hasMin);
        band.GetMaximum(out max, out hasMax);
        
        if (hasMin == 1 && hasMax == 1)
        {
            Debug.Log("It HAS :: " + string.Format("Min={0:N2}, Max={1:N2}", min, max));
        }
        else
        {
            double mean, stdDev;

            CPLErr err = band.ComputeStatistics(false, out min, out max, out mean, out stdDev, null, null);
        }

        Debug.Log(string.Format("Min={0:N2}, Max={1:N2}", min, max));


        if (band.GetOverviewCount() > 0)
        {
            Debug.Log("Band overview count: " + band.GetOverviewCount());
        } else
        {
            Debug.Log("No overviews");
        }


        ColorTable colortable = band.GetRasterColorTable();
        if (colortable != null)
        {
            Debug.Log("Band has a color table with "  + band.GetRasterColorTable().GetCount() + " entries");
        }
        else
        {
            Debug.Log("No ColorTable");
        }

        int szx = dataset.RasterXSize;
        int szy = dataset.RasterYSize;
        
        float[] rasterData = new float[szx * szy];

        band.ReadRaster(0, 0, szx, szy, rasterData, szx, szy, 0, 0);

        StringBuilder sb = new StringBuilder();

        sb.Append("RasterData: " + rasterData.Length.ToString() + " :: ");
        for(int i =0; i < 50; i++)
        {
            sb.Append(rasterData[i].ToString() + " | ");
        }
        Debug.Log(sb.ToString());

        return rasterData;
    }

}

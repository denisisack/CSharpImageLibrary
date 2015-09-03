﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsefulThings;
using static CSharpImageLibrary.DDSGeneral;

namespace CSharpImageLibrary
{
    /// <summary>
    /// Provides RGBA (DDS) format functionality.
    /// </summary>
    internal static class RGBA
    {
        /// <summary>
        /// Loads useful information from RGBA DDS image file.
        /// </summary>
        /// <param name="imagePath">Path to RGBA DDS file.</param>
        /// <returns>RGBA Pixel data as stream.</returns>
        internal static List<MipMap> Load(string imagePath)
        {
            using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                return Load(fs);
        }


        /// <summary>
        /// Loads useful information from RGBA DDS image stream.
        /// </summary>
        /// <param name="stream">Stream containing entire image file. NOT just pixels.</param>
        /// <returns>RGBA Pixel data as stream.</returns>
        internal static List<MipMap> Load(Stream stream)
        {
            DDS_HEADER header = null;
            Format format = ImageFormats.ParseDDSFormat(stream, out header);

            List<MipMap> MipMaps = new List<MipMap>();
            int newWidth = header.dwWidth;
            int newHeight = header.dwHeight;

            for (int m = 0; m < header.dwMipMapCount; m++)
            {
                // KFreon: Uncompressed, so can just read from stream.
                int mipLength = newWidth * newHeight * 4;
                MemoryStream mipmap = UsefulThings.RecyclableMemoryManager.GetStream(mipLength);
                mipmap.ReadFrom(stream, mipLength);

                MipMaps.Add(new MipMap(mipmap, newWidth, newHeight));

                newWidth /= 2;
                newHeight /= 2;
            }
            return MipMaps;
        }
        

        /// <summary>
        /// Saves mipmaps as RGBA DDS to stream.
        /// </summary>
        /// <param name="MipMaps">Mipmaps to save.</param>
        /// <param name="destination">Image stream to save to.</param>
        /// <returns>True on success.</returns>
        internal static bool Save(List<MipMap> MipMaps, Stream destination)
        {
            var header = DDSGeneral.Build_DDS_Header(MipMaps.Count, MipMaps[0].Height, MipMaps[0].Width, ImageEngineFormat.DDS_ARGB);
            using (BinaryWriter writer = new BinaryWriter(destination, Encoding.Default, true))
                DDSGeneral.Write_DDS_Header(header, writer);

            for (int m = 0; m < MipMaps.Count; m++)
                MipMaps[m].Data.WriteTo(destination);

            return true;
        }
    }
}

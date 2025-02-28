﻿using Mapsui.Cache;
using Mapsui.Layers;
using Mapsui.Providers.Wms;
using System.Threading.Tasks;
using Mapsui.Extensions.Cache;
using Mapsui.Nts.Extensions;
using Mapsui.Providers;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using Mapsui.Extensions.Projections;
using Mapsui.Tiling.Layers;
using Mapsui.Widgets;
using NetTopologySuite.Utilities;
using static System.Net.Mime.MediaTypeNames;
using System.Resources;
using Mapsui.Samples.Common.Utilities;
using System.IO;

namespace Mapsui.Samples.Common.Maps.DataFormats;

public class WmsProjectionTilingSample : ISample
{
    static WmsProjectionTilingSample()
    {
        CacheDeployer.CopyEmbeddedResourceToFile("WmsSample.sqlite");
    }

    public string Name => " 6 WMS Projection Tiling";
    public string Category => "Data Formats";

    public async Task<Map> CreateMapAsync()
    {
        var map = new Mapsui.Map
        {
            CRS = "EPSG:3857",
        };

        // The WMS request needs a CRS
        map.Layers.Add(await CreateLayerAsync());
        return map;
    }

    public static async Task<ILayer> CreateLayerAsync()
    {
        var dotSpatialProjection = new DotSpatialProjection();
        // Projection definition from https://epsg.io/6706
        dotSpatialProjection.Register("EPSG:6706",
            """
            GEOGCS["RDN2008",DATUM["Rete_Dinamica_Nazionale_2008",SPHEROID["GRS 1980",6378137,298.257222101,AUTHORITY["EPSG","7019"]],AUTHORITY["EPSG","1132"]],PRIMEM["Greenwich",0,AUTHORITY["EPSG","8901"]],UNIT["degree",0.0174532925199433,AUTHORITY["EPSG","9122"]],AUTHORITY["EPSG","6706"]]
            """);
        var dataSource = new ProjectingProvider(await CreateWmsProviderAsync(), dotSpatialProjection)
        {
            CRS = "EPSG:3857"
        };
        
        var imageLayer = new ImageLayer("mainmap")
        {
            DataSource = dataSource,
            Style = new RasterStyle(),
            Attribution = new Hyperlink()
            {
                Text = "@Agenzia delle Entrate 2023",
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Left,
                MarginX = 10,
                MarginY = 10,
                PaddingX = 4,
                PaddingY = 4,
                BackColor = Color.LightGray,
            }
        };

        return new RasterizingTileLayer(imageLayer);
    }

    private static async Task<WmsProvider> CreateWmsProviderAsync()
    {
        var wmsCachePath = Path.Combine(CacheDeployer.CacheLocation, "WmsSample");
        ////var testPath = @"C:\Github\Mapsui\Tests\Mapsui.Rendering.Skia.Tests\Resources\Cache\WmsSample";
        var persistentCache = new SqlitePersistentCache(wmsCachePath);
        const string wmsUrl = "https://wms.cartografia.agenziaentrate.gov.it/inspire/wms/ows01.php?service=WMS&request=getcapabilities";
        var provider = await WmsProvider.CreateAsync(wmsUrl, persistentCache: persistentCache);
        provider.ContinueOnError = true;
        provider.TimeOut = 20000;
        provider.CRS = "EPSG:6706";
        provider.AddLayer("province");
        provider.SetImageFormat((provider.OutputFormats)[0]);
        provider.Transparent = null;
        return provider;
    }
}

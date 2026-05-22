# Changes to run OpenSearch
## SearchDesigner in Modules.DDM.Ui.AdminController.cs
```csharp
private ISearchDesigner GetSearchDesigner()
{
    if (Session["SearchDesigner"] != null)
    {
        return (ISearchDesigner)Session["SearchDesigner"];
    }
    return new BExIS.Ddm.Providers.OpenSearch.SearchDesigner();
    //return new SearchDesigner();
}
```
## SearchProvider in BExIS.Modules.Ddm.UI.DdmModule.cs
- Replace given line with this:
```csharp
public override void Start()
{
    base.Start();
    Vaiona.IoC.IoCFactory.Container.RegisterHeirarchical(typeof(BExIS.Ddm.Api.ISearchProvider), typeof(BExIS.Ddm.Providers.OpenSearch.SearchProvider));
    //Vaiona.IoC.IoCFactory.Container.RegisterHeirarchical(typeof(BExIS.Ddm.Api.ISearchProvider), typeof(BExIS.Ddm.Providers.LuceneProvider.SearchProvider));
}
```

# New Config based on EntityTemplates
- die vorhandene SearchConfig.json hat noch EntityTemplate uebergreifende Pfade enthalten, muss gefixed werden
- Flag ob etwas in den Index darf ist bisher nur Global vorhanden
- `properties` sind nicht unterstuetzt
- `index_not_completed_metadata` nicht unterstuetzt
- globale spatial Data not supported
- lokale spatial sollte weitesgehend klappen
- aggregated primary data not supported
- string basierte primary data sollte klappen, Methode: `AppendPrimaryData(...)`

# Integraded Index
- dazugehoeriger Configteil wird korrekt gelesen
- sucht im hinterlegten Pfad nach allen xml-Files auch in Unterordnern
- Problem: XML-Dateien und in der Config hinterlegte Pfade passen nicht. Ich vermute das der DatasetManager Namespaces, Zwischenknoten etc. managed
- 

## IntegratedSourceToIdx(long entityTemplateId, XmlDocument metadata)
- Problem: Welche ID sollte fuer externe Datasets vergeben werden, wenn sie in der DB nicht vorkommen, kann es zu Kolisionen kommen
- zur Testzwecken wurde eine feste ID `99999999` vergeben
- braucht eigene Methode weil kein DatasetManager genutzt wird, der Rest ist identisch --> evtl. Flag in WriteToIndex()-Methode um "Duplikat" zu vermeiden


# Geospatial
- Funktionen vorhanden
- bisher nicht getestet
- Config wird korrekt geladen und findet Pfade
- OpenSearch Mapping vorhanden
- `AutoCompleteDocument` beinhaltet eine Methode `AddLocation`, wird bisher aber nicht genutzt

INGESTION
---

## AppendGeoPointSpatialData-Methode
- GeoPoints to Index
- Parameter:
    - LocalConfig localConfig,
    - XmlDocument metadata,
    - Dictionary<string, object> doc,
- legt die an den xpaths gefundenen Werte als Double (siehe GetDouble-Methode) im Index ab im Feld `gen_geopoint`

## AppendBoundingBoxSpatialData-Methode
- Analog zu AppendGeoPointSpatialData-Methode
- legt Double-Werte im Feld `gen_geobbox` ab

## GetDouble-HelperMethode
- Geodaten im XML-Metadaten liegen evtl. als String vor
- Helper-Methode um string-Werte in Double zu parsen

---
SEARCH
---

## SearchWithGeo-Method
- versuch die normale Suche mit Geodaten zu vereinen
- Konzept sah das aber nicht vor
- Konzept sah vor das die Suche nach Geodaten durch einen extra Filter beauftragt wird, aehnlich der Suche nach Categories
- nicht getestet
- zu bevorzugen sind:
    - siehe GeoPointSearch-Methode
    - siehe GeoBoundingBoxSearch-Methode

## GeoPointSearch-Methode
- Return: `IEnumerable<Dictionary<string, object>>`
- Parameter: 
    - QueryContainer baseQuery,
    - double latitude,
    - double longitude,
    - double radiusKm,
    - int size = 1000
- sucht im Feld `gen_geopoint`

Beispiel:
```csharp
var results = GeoPointSearch(
    query,
    latitude: 50.9,
    longitude: 11.5,
    radiusKm: 10
);
```

## GeoBoundingBoxSearch-Methode
- Return: `IEnumerable<Dictionary<string, object>>`
- Parameter:
    - QueryContainer baseQuery,
    - double topLeftLat,
    - double topLeftLon,
    - double bottomRightLat,
    - double bottomRightLon,
    - int size = 1000
- sucht im Feld `gen_geobox`

Beispiel:
```csharp
var results = GeoBoundingBoxSearch(
    query,
    topLeftLat: 51.0,
    topLeftLon: 11.0,
    bottomRightLat: 50.0,
    bottomRightLon: 12.0
);
```


# Tipps and Tricks

## Delete Index via PowerShell
```powershell
curl.exe -X DELETE "https://localhost:9200/bexissearchindex" `
    -u "admin:BExIS2_OS#" `
    -k

```


## Search for Keyword via PowerShell
Das -k muss dabei sein, sonst gibt es ein `Authority Error`
Erklärung:
	Für eine Suche im Index `bexissearchindex` muss hinten `_search` angegeben werden
	Es wird in dem Feld: `doc_id` mit der ID 11 

```bash
curl -X GET "https://localhost:9200/bexissearchindex/_search" \
    -k \
    -u 'admin:BExIS2_OS#' \
    -H 'Content-Type: application/json' \
    -d '{
        "query": {
            "match": {
                "doc_id": 12
            }
        }
    }'
```


## OpenSearch Dashboard look inside an Index
1. Create an Index Pattern (Data View)
Before you can see any data, you need to tell Dashboards which index you want to monitor:

- Open OpenSearch Dashboards in your browser (default: http://localhost:5601).
- Click the Menu icon (three horizontal lines in the top-left corner).
- Scroll to the bottom and go to Management -> Dashboards Management.
- In the left sidebar, select Index Patterns (often called Data Views in newer versions).
- Click Create index pattern.
- Enter the name of your index (e.g., bexissearchindex).
- Click Next step.
- Under Time field, either select your date field (e.g., gen_modifieddate) or select "I don't want to use the time filter" if you don't need a timeline.
- Click Create index pattern.

2. View Data in "Discover"
Now that the pattern exists, you can view your entries:

- Click the Menu icon again (top-left).
- Select Discover at the very top.
- Ensure your new index pattern (bexissearchindex) is selected in the dropdown menu on the top left.
- Important: If you don't see any data, change the time range in the top-right corner (e.g., from "Last 15 minutes" to "Last 1 year" or "Max"), as new indexes often don't have recent timestamps.

3. Alternative: Dev Tools
If you want to quickly check if data is present without configuring a UI view:
- Go to Management -> Dev Tools in the menu.
- Enter the following command and click the Play button:

```
GET /bexissearchindex/_search
{
  "query": { "match_all": {} }
}
```

**Note:** If the index is still completely empty, "Discover" will display: "No results match your search criteria".

# Steps Deployment
- save Docker Compose file to your desired location
- used Version: 3.0.0
- open PowerShell as admin and navigate to this directory
- run the command: `docker compose up -d`
- the `-d` flag stands for **detached mode**, which runs the containers in the background
- create index: `bexissearchindex`


# Data Persistent
- **Warning:** No volume has been specified
- the docker container uses the "Writeable Layer" to store data
- this behavior is **not persistent**; all data will be lost if the container is removed (e.g., via `docker compose down`)


# Create Search Index
- open PowerShell as Admin
- insert the following:
```powershell
# 1. Das Mapping definieren (Variable füllen)
$mappingJson = @"
{
  "mappings": {
    "dynamic_templates": [
      {
        "facet_fields": {
          "match": "facet_*",
          "mapping": {
            "type": "text",
            "fields": {
              "keyword": {
                "type": "keyword",
                "ignore_above": 256
              }
            }
          }
        }
      },
      {
        "property_fields": {
          "match": "property_*",
          "mapping": {
            "type": "text",
            "fields": {
              "keyword": {
                "type": "keyword",
                "ignore_above": 256
              }
            }
          }
        }
      },
      {
        "property_numeric_fields": {
          "match": "property_numeric_*",
          "mapping": {
            "type": "double"
          }
        }
      },
      {
        "category_fields": {
          "match": "category_*",
          "mapping": {
            "type": "keyword"
          }
        }
      },
      {
        "ng_fields": {
          "match": "ng_*",
          "mapping": {
            "type": "text"
          }
        }
      }
    ],
    "properties": {
      "doc_id": {
        "type": "keyword"
      },
      "gen_isPublic": {
        "type": "boolean"
      },
      "gen_entity_name": {
        "type": "keyword"
      },
      "gen_doi": {
        "type": "keyword"
      },
      "gen_modifieddate": {
        "type": "date",
        "format": "strict_date_optional_time||yyyy-MM-dd"
      },
      "gen_entitytemplate": {
        "type": "keyword"
      },
      "ng_all": {
        "type": "text"
      },
      "ng_id": {
        "type": "keyword"
      },
      "gen_geopoint": {
        "type": "geo_point"
      },

      "gen_geobbox": {
        "type": "geo_shape"
      },

      "location_radius": {
        "type": "double"
      }
    }
  }
}
"@

# 2. Den Befehl ausführen (Mapping wird via Pipeline übergeben)
$mappingJson | curl.exe -X PUT "https://localhost:9200/bexissearchindex2" `
    -u "admin:BExIS2_OS#" `
    -k `
    -H "Content-Type: application/json" `
    --data-binary "@-"

```

# Create Index for Autocompletion
- after creating the default bexis search index, you have to create the Autocomplete index
- in PowerShell insert:

```powershell

$json = @"
{
  "mappings": {
    "properties": {
      "id": {
        "type": "keyword"
      },
      "all": {
        "type": "completion"
      },
      "suggest": {
        "type": "completion",
        "contexts": [
          {
            "name": "category",
            "type": "category"
          }
        ]
      }
    }
  }
}
"@

$json | curl.exe -X PUT "https://localhost:9200/autocompleteindex" `
    -u "admin:BExIS2_OS#" `
    -k `
    -H "Content-Type: application/json" `
    --data-binary "@-"
```


# OpenSearch Dashboard
- mit diesen Settings ist das OpenSearch Dashboard erreichbar ueber: http://localhost:5601/
# Creating and Executing Queries within Evolution

## Creating a Query

You can perform a POST on the `/documents/queries` endpoint to create a new query.
This could include a custom query, or it could specify document types to be run with
the query. All of the information needed to run this query will be contained within
the request body of this POST on `/documents/queries`. There are five main pieces of
information that can be included in this request body:

- Query Type:
    A QueryType can either have type DocumentType or CustomQuery. Multiple
    DocumentType ids can be added, but only one CustomQuery id may be present. The
    QueryType property is required.

    Acceptable:
    ```json
    {
        "queryType": [
            {
                "type": "CustomQuery",
                "ids": ["121"]
            }
        ]
    }
    ```

    Acceptable:
    ```json
    {
        "queryType": [
            {
                "type": "DocumentType",
                "ids": ["101, 202"]
            }
        ]
    }
    ```

    Unacceptable:
    ```json
    {
        "queryType": [
            {
                "type": "CustomQuery",
                "ids": ["121, 114"]
            }
        ]
    }
    ```

    If you would like to see the possible custom queries that you could execute, you can
    perform a GET on the `/custom-queries` endpoint. If you wanted to see the keyword types
    associated with a particular custom query, you could perform a GET on the
    `/custom-queries/{customQueryId}/keyword-types` endpoint.
- Max Results:
    This property limits the number of results returned when retrieving the list of
    document results using the `/documents/queries/{queryId}/results` endpoint. If this
    property is not used, all results will be returned.
- Query Keywords:
    Using this property you can add a collection of query keywords to your query. Each
    query keyword can contain an operator (Equal, LessThan, etc.) and a relation (And,
    Or, To). Note: not all operators are supported for all keyword data types. Additionally, relations only apply to keywords of the same keyword type.

    For instance, the following example will not actually "Or" the keywords, because they are not the same keyword type:

    ```json
    {
        "queryType": [
            {
                "type": "CustomQuery",
                "ids": ["121"]
            }
        ],
        "queryKeywordCollection": [
            {
                "typeId": "170",
                "value": "OFFICE MAX",
                "operator": "Equal",
                "relation": "Or"
            },
            {
                "typeId": "111",
                "value": "LOST",
                "operator": "NotEqual",
                "relation": "Or"
            }
        ]
    }
    ```

    However, this example will "Or" the two keywords.

    ```json
    {
        "queryType": [
            {
                "type": "CustomQuery",
                "ids": ["121"]
            }
        ],
        "queryKeywordCollection": [
            {
                "typeId": "170",
                "value": "OFFICE MAX",
                "operator": "Equal",
                "relation": "Or"
            },
            {
                "typeId": "170",
                "value": "HYLAND",
                "operator": "NotEqual",
                "relation": "Or"
            }
        ]
    }
    ```
- Date Ranges:
    This property allows you to limit the results of a query to only those documents
    that have a document date that falls within a given range. If a range is provided
    that does not contain a `start`, the range will be interpreted as any date that
    occurs before the `end` date. If a range is provided that does not contain an
    `end`, the range will be interpreted as any date that occurs after the `start`
    date. If multiple ranges are provided, they will all be applied. For example, if you wanted
    all documents that have a document date from either 2016 or 2018, you could use the
    following request:

    ```json
    {
        "queryType": [
            {
                "type": "CustomQuery",
                "ids": ["121"]
            }
        ],
        "documentDateRangeCollection": [
            {
                "start": "01/01/2016",
                "end": "12/31/2016"
            },
            {
                "start": "01/01/2018",
                "end": "12/31/2018"
            }
        ]
    }
    ```
- User Defined Display Columns:
    Using this property, you can configure the query to return the exact
    display columns you would like. If the query has pre-configured display columns, the user
    defined display columns will be added to the pre-configured display columns in the
    response. If a query has no pre-configured display columns and this property is not
    included on the request, a single display column will be added to the response by default
    containing the name of the each document in the document result list.

The response to this request will contain the id of the query. This id can then be
used to retrieve the document results (`/documents/queries/{queryId}/results`) or to
get the display column configuration for the query (`/documents/queries/{queryId}/columns`).
A Location header will also be included on the response with the url for the document
results.

```http
POST /documents/queries HTTP/1.1
Content-Type: application/json

{
  "queryType": [
    {
      "type": "CustomQuery",
      "ids": ["121"]
    }
  ],
  "maxResults": 100,
  "queryKeywordCollection": [
    {
      "typeId": "170",
      "value": "OFFICE MAX",
      "operator": "Equal",
      "relation": "AND"
    }
  ],
  "documentDateRangeCollection": [
    {
      "start": "07/01/2018",
      "end": "07/31/2018"
    }
  ],
  "userDisplayColumns": [
    {
      "displayColumnType": "Keyword",
      "keywordTypeId": "170"
    },
    {
      "displayColumnType": "DocumentName"
    }
  ]
}
```
```http
HTTP/1.1 201 Created
Content-Type: application/json
Location: {hostName}/documents/queries/qwerty/results

{
    "id": "qwerty"
}
```

## Retrieving the Expected Count of a Query Results

If you'd like to know the expected count of the results of the query you can add the
`Hyland-Include-Item-Count` custom request header to this request. The value of this
header should be set to `true` if you would like the expected count to be included in
the custom response header `Hyland-Item-Count`. If the `Hyland-Include-Item-Count`
header value is set to `false`, or if the header is left off the request, the
`Hyland-Item-Count` header will not be included on the response.

```http
POST /documents/queries HTTP/1.1
Content-Type: application/json
Hyland-Include-Item-Count: true

{
  "queryType": [
    {
      "type": "CustomQuery",
      "ids": ["121"]
    }
  ]
}
```
```http
HTTP/1.1 201 Created
Content-Type: application/json
Location: {hostName}/documents/queries/qwerty/results
Hyland-Item-Count: 10

{
    "id": "qwerty"
}
```

The count included in the `Hyland-Item-Count` header is an estimate. When retrieving the
actual results you might not get the same number. Additionally, adding
`Hyland-Include-Item-Count` will incur additional performance overhead when executing this
request. If the `/documents/queries/{queryId}/results` endpoint will generally be called
immediately after this, then including this header is not recommended. Instead, the accurate
count can be calculated based on the response from the `/documents/queries/{queryId}/results`
endpoint and the `/documents/queries` endpoint will not have to experience decreased
performance. The use of `Hyland-Include-Item-Count` can be very helpful if retrieving the full
results of the query may not be done immediately or ever at all.

## Retrieving a Query's Results

Once you have the id of a query, you can retrieve the document results of this query.
This is done by performing a GET on `/documents/queries/{queryId}/results`, which is
the url that will be included in the Location header of the POST response of
`/documents/queries`.

```http
GET /documents/queries/qwerty/results HTTP/1.1
```
```http
HTTP/1.1 200 Ok
Content-Type: application/json

{
    "documentResultCollection": [
        {
            "displayColumns": [
                {
                    "index": "0",
                    "values": [
                        "AP - Checks"
                    ]
                },
                {
                    "index": "1",
                    "values": [
                        "2016-06-20"
                    ]
                },
                {
                    "index": "2",
                    "values": [
                        "767"
                    ]
                }
            ],
            "id": "2725"
        }
    ]
}
```

The document results response contains the document id and the display column values
of each document result. Notice that the `values` property is an array. This is for when a display column represents a keyword that is on a document multiple times. For example, a document with multiple phone number keywords on it might look like the following:

```json
{
    "documentResultCollection": [
        {
            "displayColumns": [
                {
                    "index": "0",
                    "values": [
                        "440-123-4567",
                        "216-987-6543"
                    ]
                }
            ],
            "id": "2725"
        }
    ]
}
```

Along with each display column value, an index of the display
column is included. If you needed to get the configuration of a particular display
column you could perform a GET on `/documents/queries/{queryId}/columns` and
then find the configuration within the response that has the same index value.

```http
GET /documents/queries/qwerty/columns HTTP/1.1
```
```http
HTTP/1.1 200 Ok
Content-Type: application/json

{
    "displayColumns": [
        {
            "dataType": "Alphanumeric",
            "heading": "Document Type",
            "index": "0",
            "type": "Attribute"
        },
        {
            "dataType": "Date",
            "heading": "Document Date",
            "index": "1",
            "type": "Attribute"
        },
        {
            "dataType": "Alphanumeric",
            "heading": "PO Number",
            "index": "2",
            "keywordTypeId": "277",
            "type": "Keyword"
        }
    ]
}
```

Separating the configuration data from the column values prevents the configuration
data from being duplicated in a response. Also contained in the configuration data is
the heading, the data type, and the display column type. Additionally, if the type is
equal to Keyword the keywordTypeId will be included.

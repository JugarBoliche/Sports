# Document Retrieval in Evolution
## Table of Contents
* [Documents, Revision, Renditions](#documents-revisions-renditions)
    * [Document](#document)
    * [Revision](#revision)
        * [Latest Revision](#latest-revision)
    * [Rendition](#rendition)
        * [Default Rendition](#default-rendition)
* [Content](#document-content)
    * [Accept Header](#accept-header)
        * [Media Types](#media-types)
    * [Pages Query Parameter](#pages-query-parameter)
        * [Default Media Type](#default-media-type)
    * [Supported Media Types](#supported-media-types)
        * [Entire Document](#entire-document)
        * [Single Page](#single-page)
* [How to Use](#how-to-use)
    * [HEAD Request](#head-request)
        * [Without Page Query Parameter](#without-page-query)
        * [With Page Query Parameter](#with-page-query)
    * [Retrieving Entire Document](#retrieving-entire-document)
    * [Retrieving Single Page](#retrieving-single-page)
    * [Requesting a Page that does not Exist](#nonexistent-page)

<a name="documents-revisions-renditions"></a>
## Documents, Revisions, Renditions
A single document is broken down into three separate layers. These layers are formally designated Document, Revision, and Rendition. The tree below shows the relationship between the layers.

![Document-Revision-Rendition-Structure](./images/UnityRevRend.gif)
<a name="document"></a>
#### Document
A `document` resource provides metadata about the document. To request a document, you must specify the `{documentId}` for that document.

The endpoint for retrieving metadata for a document is `/documents/{documentId}`.

<a name="revision"></a>
#### Revision
A revision is a container for the contents of a document. A new revision of the document is stored each time a document is changed. A document consists of one or more revisions. To request a specific revision of a document, you must specify the `{revisionId}` for that revision. If an OnBase system does not have EDM Services or revisions is not configured for a given document type, the concept of revisions still applies. Every document still has at least one revision.

The endpoint for retrieivng the list of revisions of a document is `/documents/{documentId}/revisions`. The endpoint for retrieving a specific revision is `/documents/{documentId}/revisions/{revisionId}`.
<a name="latest-revision"></a>
##### Latest Revision
To get the latest revision of a document, you can request the `latest` resource in place of `{revisionId}`.

Example: `/documents/{documentId}/revisions/latest`
<a name="rendition"></a>
#### Rendition
A rendition is a version of an original document that is saved in a different File Format. A document revision consists of one or more renditions. The `rendition` resource provides metadata about the rendition. To request a specific rendition of a document revision, you must specify the `{fileTypeId}` for that rendition.

The endpoint for retrieving the list of renditions of a revision is `/documents/{documentId}/revisions/{revisionId}/renditions`. The endpoint for retrieving a specific rendition is `/documents/{documentId}/revisions/{revisionId}/renditions/{fileTypeId}`.
<a name="default-rendition"></a>
##### Default Rendition
To get the default rendition of a document revision, you can request the `default` resource in place of `{fileTypeId}`.

Example: `/documents/{documentId}/revisions/{revisionId}/renditions/default`

The default rendition can be used in conjunction with the latest revision. This is the best way to get the document, unless more specific revisions and renditions are needed.

Example: `/documents/{documentId}/revisions/latest/renditions/default`

<a name="document-content"></a>
## Content
The `content` resource provides document bytes for the specified rendition.

The endpoint is `/documents/{documentId}/revisions/{revisionId}/renditions/{fileTypeId}/content`.

<a name="accept-header"></a>
### Accept Header
The `Accept` header is used to request a particular file format. The `content` resource supports [proactive negotiation](https://tools.ietf.org/html/rfc7231#section-3.4.1).
Content negotiation allows the client to specify the list of formats they would prefer to receive or specify which to ignore if unable to process particular formats. If the document cannot be retrieved in any of the given formats, `406 Not Acceptable` will be returned.
<a name="media-types"></a>
#### Media Types
[Media types](https://tools.ietf.org/html/rfc6838) are used to specify the file format of the document.

To get the default media type of the document, either omit the `Accept` header or use the full wild card media type: `*/*`.

Subtype wildcards can be used to retrieve the best media type of a particular type. The following subtype wildcards are supported by Evolution: `image/*`, `application/*`, and `text/*`.

You can try to retrieve a document in a specific media type. The following media types can be requested: `image/tiff`, `image/png`, `image/jpeg`, `application/pdf`, and `text/plain`.

If Evolution does not support a requested media type or a document cannot be retrieved as the specified media type, a `406 Not Acceptable` will be returned.

<a name="pages-query-parameter"></a>
### Pages Query Parameter
Individual pages can be retrieved using the `pages` query parameter. This is done by adding `?pages=value` at the end of the URL. Currently, only a single page number is supported. Page ranges are not currently supported. If the page number does not exist or the value provided is not a single page number, a `404 Not Found` will be returned.

<a name="default-media-type"></a>
#### Default Media Type

The ability to retrieve individual pages is only supported by a few file types (Tiff and text files) when using the default media type (`*/*` or omitted Accept Header). The entire document may be returned when requesting page 1 and the default media type if a single page cannot be retrieved due to the media type in the response. A `404 Not Found` will be returned if a single page cannot be retrieved and a page number greater than one is requested.

<a name="supported-media-types"></a>
### Supported Media Types
<a name="entire-document"></a>
#### Entire Document
The following table specifies which media types can be used on which file types when retrieving the entire document as a single file.

| FileType | `*/*` |`image/tiff` | `image/png` | `image/jpeg` | `application/pdf` | `text/plain` |
|---|---|---|---|---|---|---|
| Image | Supported | Supported | Not Supported | Not Supported | Supported | Not Supported |
| PDF | Supported | Supported | Not Supported | Not Supported | Supported | Not Supported |
| Text | Supported | Supported | Not Supported | Not Supported | Supported | Supported |
| MS Word | Supported | Supported | Not Supported | Not Supported | Supported | Not Supported |
| MS Excel | Supported | Supported | Not Supported | Not Supported | Not Supported | Not Supported |

<a name="single-page"></a>
#### Single Page
The following table specifies which media types can be used on which file types when retrieving individual pages.

| FileType | `*/*` | `image/tiff` | `image/png` | `image/jpeg` | `application/pdf` | `text/plain` |
|---|---|---|---|---|---|---|
| Image | Supported |Supported | Supported | Supported | Supported | Not Supported |
| PDF | Supported* | Supported | Supported | Supported | Supported | Not Supported |
| Text | Supported | Supported | Supported | Supported | Supported | Supported |
| MS Word | Supported* | Supported | Supported | Supported | Supported | Supported | Not Supported |
| MS Excel | Supported* |Supported | Supported | Supported | Supported | Not Supported |

*Retrieving individual pages is not supported with this file type. Requesting `*/*` (or omitting Accept header) and page 1 will return the entire document as a single file. Any other page number will result in `404 Not Found`.

<a name="how-to-use"></a>
## How to Use
<a name="head-request"></a>
#### HEAD Request
The `HEAD` Request can be used to get information about the content before getting actual content bytes. This can be useful for getting the number of pages (must include the `pages` query parameter), previewing the negotiated content type, and previewing the content length. If the pages query parameter is included in the request, `Hyland-Item-Count` header will be included on the response.

<a name="without-page-query"></a>
##### Without Page Query Parameter

```http
HEAD /documents/101/revisions/1/renditions/2/content HTTP/1.1
Authorization: Bearer valid-token
Accept: image/tiff
```
```http
HTTP/1.1 200 OK
Content-Length: {bytes length}
Content-Type: image/tiff
```
<a name="with-page-query"></a>
##### With Page Query Parameter
```http
HEAD /documents/101/revisions/1/renditions/2/content?pages=1 HTTP/1.1
Authorization: Bearer valid-token
Accept: image/tiff
```
```http
HTTP/1.1 200 OK
Content-Length: {bytes length}
Content-Type: image/tiff
Hyland-Item-Count: 3
```
<a name="retrieving-entire-document"></a>
#### Retrieving Entire Document
The best approach is to use `latest` for the revision ID and `default` for the file type ID unless a specific revision or rendition is known to be needed. If you simply want to retrieve the document as a single file, specifying a default media type and no pages is the best approach, as a document will be returned successfully every time.

This example shows the best approach:
```http
GET /documents/101/revisions/latest/renditions/default/content HTTP/1.1
Authorization: Bearer valid-token
Accept: */*
```
```http
HTTP/1.1 200 OK
Content-Length: {byte length}
Content-Type: image/tiff

[bytes for the entire document]
```

This example shows how to specify a revision and rendition:
```http
GET /documents/101/revisions/1/renditions/2/content HTTP/1.1
Authorization: Bearer valid-token
Accept: image/tiff
```
```http
HTTP/1.1 200 OK
Content-Length: {bytes length}
Content-Type: image/tiff

[bytes for the entire document]
```
<a name="retrieving-single-page"></a>
#### Retrieving Single Page
```http
GET /documents/101/revisions/1/renditions/2/content?pages=2 HTTP/1.1
Authorization: Bearer valid-token
Accept: image/tiff
```
```http
HTTP/1.1 200 OK
Content-Length: {bytes length}
Content-Type: image/tiff
Hyland-Item-Count: 3

[bytes for the 2nd page of the document]
```
<a name="nonexistent-page"></a>
#### Requesting a Page that does not Exist
This example document has 3 pages, so the request below will return `404 Not Found`.
```http
GET /documents/101/revisions/latest/renditions/default/content?pages=4 HTTP/1.1
Authorization: Bearer valid-token
Accept: image/tiff
```
```http
HTTP/1.1 404 Not Found
Content-Type: application/problem+json

{
    "title": "Not Found",
    "status": 404
}
```

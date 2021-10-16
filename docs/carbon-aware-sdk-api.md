## Title

\<*Additional information about your API call. Try to use verbs that match both
request type (fetching vs modifying) and plurality (one vs multiple).*\>

### URL

>   \<*The URL Structure (path only, no root url)*\>

### Method:

>   \<*The request type*\>

>   `GET` \| `POST` \| `DELETE` \| `PUT`

### Params

| Name     | Type         | Required / Optional |
|----------|--------------|---------------------|
| id       | Integer      | Required            |
| photo_id | Alphanumeric | Optional            |

### Success Response:

>   \<*What should the status code be on success and is there any returned data?
>   This is useful when people need to to know what their callbacks should
>   expect!*\>

-   **Code:** 200 **Content:** `{ id : 12 }`

### Error Response:

>   \<*Most endpoints will have many ways they can fail. From unauthorized
>   access, to wrongful parameters etc. All of those should be liste d here. It
>   might seem repetitive, but it helps prevent assumptions from being made
>   where they should be.*\>

-   **Code:** 401 UNAUTHORIZED **Content:** `{ error : "Log in" }`

    OR

    -   **Code:** 422 UNPROCESSABLE ENTRY **Content:** `{ error : "Email
        Invalid" }`

### Sample Call:

>   \<*Just a sample call to your endpoint in a runnable format (\$.ajax call or
>   a curl request) - this makes life easier and more predictable.*\>

### Notes:

>   \<*This is where all uncertainties, commentary, discussion etc. can go. I
>   recommend timestamping and identifying oneself when leaving comments
>   here.*\>

# HyenaORM
A learning exercise in making a homebrew ORM

**Goal:**

Make a flexible ORM.

**Approach:**

Initially it will support a single database configuration under MSSQL, the goal will be to expand this out into supporting other RDBMS and allow for multiple database configurations.

Currently only supports single-level tables.

Will include support for complex structures as time progresses.

**Contains:**

**_TableNameAttribute:_** Store the name of the table relating to the class;

**_FieldNameAttribute:_** Store the name of the property's corresponding database field;

**_PrimaryKeyAttribute:_** Mark the property that is the primary key of the table;

**_GetAll():_** Returns all records of a given class;

**_GetRecordByPrimaryKey(object primaryKeyValue):_** Returns the class record containing the primary key value;

# HyenaORM
A learning exercise in making a homebrew ORM

**Goal:**

Make a flexible ORM.

**Approach:**

Initially it will support a single database configuration under MSSQL. An IDatabase interface provides the option for building solutions for other RDBMS.

Currently only supports selects from single-level tables.

Creates, Updates and Deletes are in the pipeline for future development.

**Contains:**

**_TableNameAttribute:_** Store the name of the table relating to the class;

**_FieldNameAttribute:_** Store the name of the property's corresponding database field;

**_PrimaryKeyAttribute:_** Mark the property that is the primary key of the table;

**_Task<IEnumerable<T>> LoadAllRecords<T>():_** Returns all records of a given class;

**_Task<T> LoadRecordByPrimaryKey<T>(object primaryKey):_** Returns the class record containing the primary key value;

**Requirements:**

System.Data.SqlClient (4.6.1) is required in any project that utilises this project.
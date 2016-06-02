create table logs
(
	id bigint identity(1, 1) primary key,
	name varchar(2000),
)

create table categories
(
	id bigint primary key,
	name varchar(2000),
)

create table products
(
	id bigint primary key,
	price float not null,
	name varchar(2000),
	categoryId bigint not null constraint products_categoryId_FK FOREIGN KEY ( categoryId ) references categories(id)
)
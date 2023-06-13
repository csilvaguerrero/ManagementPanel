CREATE DATABASE DB_Management;

USE DB_Management;

CREATE TABLE Equipos(
	IdEquipo INT PRIMARY KEY IDENTITY(1,1),
	Nombre varchar(50) NOT NULL UNIQUE,
	Departamento varchar(50) NOT NULL,
	Descripcion varchar(255)
);

--ALTER TABLE Equipos ADD CONSTRAINT ck_depto CHECK (Departamento='Desarollo' OR Departamento='Ampliaciones');

CREATE TABLE Usuarios(
	IdUsuario INT PRIMARY KEY IDENTITY(1,1),
	Nombre VARCHAR(50) NOT NULL,
	Apellidos VARCHAR(100) NOT NULL,
	Equipo VARCHAR(50) NULL,
	Usuario VARCHAR(50) NOT NULL UNIQUE,
	Contrasenna VARCHAR(255) NOT NULL,
	Admin BIT NOT NULL
);

ALTER TABLE Usuarios ADD CONSTRAINT fk_equipos FOREIGN KEY (Equipo) REFERENCES Equipos(Nombre) ON UPDATE CASCADE ON DELETE CASCADE;

INSERT INTO Equipos(Nombre,Departamento,Descripcion) VALUES('Equipo1','Desarollo','');

INSERT INTO Usuarios(Nombre,Apellidos,Equipo,Usuario,Contrasenna,Admin) VALUES('admin','admin','Equipo1','admin','admin','1');

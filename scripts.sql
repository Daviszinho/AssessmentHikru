--Connection to Oracle Autonomous Database
--(description= (retry_count=20)(retry_delay=3)(address=(protocol=tcps)(port=1522)(host=adb.mx-queretaro-1.oraclecloud.com))(connect_data=(service_name=g2611a32d6a01f3_oracletest_high.adb.oraclecloud.com))(security=(ssl_server_dn_match=yes)))



CREATE TABLE Recruiter (
    RecruiterId NUMBER PRIMARY KEY,
    Name VARCHAR2(255) NOT NULL,
    Email VARCHAR2(255) UNIQUE NOT NULL
);

CREATE TABLE Department (
    DepartmentId NUMBER PRIMARY KEY,
    Name VARCHAR2(255) NOT NULL,
    ManagerId NUMBER, -- Opcional si hay un jefe de departamento
    CONSTRAINT fk_manager FOREIGN KEY (ManagerId) REFERENCES Recruiter(RecruiterId)
);

CREATE TABLE Position (
    PositionId NUMBER PRIMARY KEY,
    Title VARCHAR2(100) NOT NULL,
    Description VARCHAR2(1000) NOT NULL,
    Location VARCHAR2(255) NOT NULL,
    Status VARCHAR2(20) CHECK (Status IN ('draft', 'open', 'closed', 'archived')) NOT NULL,
    RecruiterId NUMBER NOT NULL,
    DepartmentId NUMBER NOT NULL,
    Budget NUMBER(12,2) NOT NULL,
    ClosingDate DATE,
    CONSTRAINT fk_recruiter FOREIGN KEY (RecruiterId) REFERENCES Recruiter(RecruiterId),
    CONSTRAINT fk_department FOREIGN KEY (DepartmentId) REFERENCES Department(DepartmentId)
);

-- Insertar Reclutadores
INSERT INTO Recruiter (RecruiterId, Name, Email) VALUES (1, 'Ana Rodríguez', 'ana.rodriguez@example.com');
INSERT INTO Recruiter (RecruiterId, Name, Email) VALUES (2, 'Carlos Gómez', 'carlos.gomez@example.com');

-- Insertar Departamentos
INSERT INTO Department (DepartmentId, Name, ManagerId) VALUES (1, 'Recursos Humanos', 1);
INSERT INTO Department (DepartmentId, Name, ManagerId) VALUES (2, 'Tecnología', 2);

-- Insertar Posiciones
INSERT INTO Position (PositionId, Title, Description, Location, Status, RecruiterId, DepartmentId, Budget, ClosingDate)
VALUES (1, 'Desarrollador Backend', 'Desarrollo de APIs con PostgreSQL y Oracle.', 'San José', 'open', 2, 2, 75000, TO_DATE('2025-06-30', 'YYYY-MM-DD'));

INSERT INTO Position (PositionId, Title, Description, Location, Status, RecruiterId, DepartmentId, Budget, ClosingDate)
VALUES (2, 'Analista de Recursos Humanos', 'Gestión de contrataciones y selección de personal.', 'Heredia', 'draft', 1, 1, 50000, NULL);

INSERT INTO Position (PositionId, Title, Description, Location, Status, RecruiterId, DepartmentId, Budget, ClosingDate)
VALUES (3, 'Administrador de Base de Datos', 'Manejo de bases de datos Oracle y optimización de consultas.', 'San José', 'closed', 2, 2, 90000, TO_DATE('2025-05-15', 'YYYY-MM-DD'));
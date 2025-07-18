CREATE TABLE Cliente (
    cliente_id INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre TEXT,
    fecha_nacimiento DATETIME,
    sexo TEXT,
    ingresos DECIMAL
);
CREATE TABLE Cuenta (
    cuenta_id INTEGER PRIMARY KEY AUTOINCREMENT,
    numero_cuenta INTEGER NOT NULL UNIQUE,
    saldo DECIMAL DEFAULT 0,
    ClienteId INTEGER NOT NULL,
    FOREIGN KEY (ClienteId) REFERENCES Cliente(cliente_id)
);

CREATE TABLE Transaccion (
    transaccion_id INTEGER PRIMARY KEY AUTOINCREMENT,
    fecha DATETIME NOT NULL,
    monto DECIMAL NOT NULL,
    cuenta_id INTEGER NOT NULL,
    numero_cuenta INTEGER NOT NULL,
    tipo TEXT, -- Puede ser "Deposito" o "Retiro"
    FOREIGN KEY (cuenta_id) REFERENCES Cuenta(cuenta_id)
);

/*ALTER TABLE Cuenta ADD COLUMN cliente_id INTEGER NOT NULL DEFAULT 1;*/
select *from Cuenta
select *from Cliente
select *from Transaccion

DELETE FROM Cliente
WHERE cliente_id = 5

DELETE FROM Cuenta

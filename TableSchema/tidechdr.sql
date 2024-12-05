
-- Equipment table mapping to a facility
CREATE TABLE tidechdr (  
    Facility NVARCHAR(50),        
    Unit INT,                     
    E_Code NVARCHAR(10),           
    Eq_Component_Tag NVARCHAR(20), 
    Equipment_Name NVARCHAR(50),  
    Equipment_Status NVARCHAR(20),
    Equip_Status_Date DATE        
); 
-- Create Consortium records
INSERT INTO "Consortia" ("ConsortiumCode") VALUES
    ('C_0002'), -- Blackpool
    ('C_0003'), -- Bristol
    ('C_0004'), -- Broadland
    ('C_0006'), -- Cambridge
    ('C_0007'), -- Cambridgeshire & Peterborough Combined Authority
    ('C_0008'), -- Cheshire East
    ('C_0010'), -- Cornwall
    ('C_0012'), -- Darlington Borough Council
    ('C_0013'), -- Dartford
    ('C_0014'), -- Devon
    ('C_0015'), -- Dorset
    ('C_0016'), -- Eden District Council
    ('C_0017'), -- Greater London Authority
    ('C_0021'), -- Lewes
    ('C_0022'), -- Liverpool City Region
    ('C_0024'), -- Midlands Net Zero Hub
    ('C_0027'), -- North Yorkshire County Council
    ('C_0029'), -- Oxfordshire County Council
    ('C_0031'), -- Portsmouth
    ('C_0033'), -- Sedgemoor
    ('C_0037'), -- Stroud
    ('C_0038'), -- Suffolk County Council
    ('C_0039'), -- Surrey County Council
    ('C_0044')  -- West Devon
ON CONFLICT DO NOTHING;

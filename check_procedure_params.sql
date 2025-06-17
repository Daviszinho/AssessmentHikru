SELECT 
    argument_name,
    data_type,
    in_out,
    data_length,
    data_precision,
    data_scale
FROM 
    all_arguments
WHERE 
    package_name IS NULL
    AND object_name = 'ADD_POSITION'
ORDER BY 
    position;

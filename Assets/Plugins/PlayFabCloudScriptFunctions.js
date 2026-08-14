handlers.setUserData = function(args,context)
{
	var data = {}; //en este dicciones vamos a guardar la data que viene de unity
	data[args.key] = args.Value //guarda automáticamente todas las llaves existentes en el diccionario data con su respectivo valor
	//hace lo mismo que se hace en C# con un for, se guarda por cada llave existente en un args, un valor en el diccionario

	//esta función es de playfab, no nuestro
	server.UpdateUserData(
	{
	PlayFabID: currentPlayerID,
	Data: data
	});

	return {
		Success: true
	};
};

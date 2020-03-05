 export const fetchData = async <T>(command: Function): Promise<T | void> => {
  try {
    const response = await command();
    if (response) {
      return response;
    }
  } catch (e) {
    console.log("something went wrong", e);
  }
};

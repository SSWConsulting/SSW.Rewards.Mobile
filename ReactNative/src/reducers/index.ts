import { combineReducers } from "redux";
import AuthReducer from './AuthReducers';
import ApiReducer from "./ApiReducer";

export default combineReducers({
    auth: AuthReducer,
    api: ApiReducer
});
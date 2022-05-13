var erc20 = artifacts.require("./StandardERC20.sol");
module.exports = function(deployer) {
      deployer.deploy(erc20, 1000000);
}
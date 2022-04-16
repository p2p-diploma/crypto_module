var transferAgreement = artifacts.require ("./TransferAgreementContract.sol");
module.exports = function(deployer) {
      deployer.deploy(transferAgreement);
}